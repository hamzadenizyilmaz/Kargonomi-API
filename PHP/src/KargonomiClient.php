<?php

declare(strict_types=1);

namespace Kargonomi;

use Kargonomi\Exception\AuthenticationException;
use Kargonomi\Exception\KargonomiException;
use Kargonomi\Exception\NetworkException;
use Kargonomi\Exception\NotFoundException;
use Kargonomi\Exception\RateLimitException;
use Kargonomi\Exception\ValidationException;
use Kargonomi\Http\CurlTransport;
use Kargonomi\Http\Request;
use Kargonomi\Http\Response;
use Kargonomi\Http\TransportInterface;
use Kargonomi\Resource\AccountResource;
use Kargonomi\Resource\BarcodesResource;
use Kargonomi\Resource\LocationsResource;
use Kargonomi\Resource\PricingResource;
use Kargonomi\Resource\ShipmentsResource;
use Kargonomi\Resource\WarehousesResource;
use Kargonomi\Resource\WebhooksResource;

final class KargonomiClient
{
    private readonly TransportInterface $transport;
    private readonly string $baseUrl;

    public function __construct(
        #[\SensitiveParameter]
        private readonly string $apiToken,
        ?string $baseUrl = null,
        ?TransportInterface $transport = null,
        private readonly int $timeoutSeconds = 30,
        private readonly int $maxRetries = 2,
    ) {
        if (trim($apiToken) === '') {
            throw new \InvalidArgumentException('apiToken must not be empty.');
        }
        $this->baseUrl = $baseUrl ?? self::configuredBaseUrl();
        if (!str_starts_with(strtolower($this->baseUrl), 'https://')) {
            throw new \InvalidArgumentException('baseUrl must use HTTPS.');
        }
        if ($timeoutSeconds < 1 || $maxRetries < 0 || $maxRetries > 5) {
            throw new \InvalidArgumentException('Invalid timeout or retry configuration.');
        }
        $this->transport = $transport ?? new CurlTransport();
    }

    private static function configuredBaseUrl(): string
    {
        $filename = dirname(__DIR__) . '/appsettings.json';
        $contents = file_get_contents($filename);
        if ($contents === false) {
            throw new \RuntimeException('Kargonomi appsettings.json could not be read.');
        }
        $settings = json_decode($contents, true, 512, JSON_THROW_ON_ERROR);
        $section = is_array($settings) ? ($settings['Kargonomi'] ?? null) : null;
        $baseUrl = is_array($section) ? ($section['BaseUrl'] ?? null) : null;
        if (!is_string($baseUrl) || $baseUrl === '') {
            throw new \RuntimeException('Kargonomi:BaseUrl is missing from appsettings.json.');
        }

        return $baseUrl;
    }

    public function shipments(): ShipmentsResource
    {
        return new ShipmentsResource($this);
    }

    public function pricing(): PricingResource
    {
        return new PricingResource($this);
    }

    public function account(): AccountResource
    {
        return new AccountResource($this);
    }

    public function warehouses(): WarehousesResource
    {
        return new WarehousesResource($this);
    }

    public function locations(): LocationsResource
    {
        return new LocationsResource($this);
    }

    public function barcodes(): BarcodesResource
    {
        return new BarcodesResource($this);
    }

    public function webhooks(): WebhooksResource
    {
        return new WebhooksResource($this);
    }

    /**
     * @internal Resource implementation entry point.
     * @param array<string, int|string> $query
     * @param array<string, mixed>|null $json
     * @param array<string, string>|null $form
     * @return array<mixed>
     */
    public function requestJson(
        string $method,
        string $path,
        array $query = [],
        ?array $json = null,
        ?array $form = null,
    ): array {
        $response = $this->send($method, $path, $query, $json, $form);
        if ($response->body === '') {
            throw new \UnexpectedValueException('Kargonomi returned an empty JSON response.');
        }
        $decoded = json_decode($response->body, true, 512, JSON_THROW_ON_ERROR);
        if (!is_array($decoded)) {
            throw new \UnexpectedValueException('Kargonomi returned an invalid JSON object.');
        }

        return $decoded;
    }

    /**
     * @internal Resource implementation entry point.
     * @param array<string, mixed>|null $json
     * @param array<string, string>|null $form
     */
    public function requestVoid(string $method, string $path, ?array $json = null, ?array $form = null): void
    {
        $this->send($method, $path, [], $json, $form);
    }

    /**
     * Verifies a provider signature against the exact bytes received before JSON parsing.
     *
     */
    public static function verifyWebhookSignature(
        string $rawBody,
        string $signature,
        #[\SensitiveParameter]
        string $secret,
        string $encoding = 'hex',
    ): bool {
        if ($encoding !== 'hex' && $encoding !== 'base64') {
            throw new \InvalidArgumentException('Signature encoding must be hex or base64.');
        }
        $binary = hash_hmac('sha256', $rawBody, $secret, true);
        $expected = match ($encoding) {
            'hex' => bin2hex($binary),
            'base64' => base64_encode($binary),
        };

        return hash_equals($expected, trim($signature));
    }

    /**
     * @param array<string, int|string> $query
     * @param array<string, mixed>|null $json
     * @param array<string, string>|null $form
     */
    private function send(string $method, string $path, array $query, ?array $json, ?array $form): Response
    {
        $request = new Request(
            strtoupper($method),
            $this->baseUrl,
            '/' . ltrim($path, '/'),
            ['Authorization' => 'Bearer ' . $this->apiToken],
            $query,
            $json,
            $form,
            timeoutSeconds: $this->timeoutSeconds,
        );

        $attempt = 0;
        while (true) {
            try {
                $response = $this->transport->send($request);
            } catch (NetworkException $exception) {
                if ($request->method !== 'GET' || $attempt >= $this->maxRetries) {
                    throw $exception;
                }
                ++$attempt;
                usleep(100_000 * (2 ** ($attempt - 1)));
                continue;
            }

            if ($request->method === 'GET' && $attempt < $this->maxRetries && ($response->status === 429 || $response->status >= 500)) {
                ++$attempt;
                usleep(100_000 * (2 ** ($attempt - 1)));
                continue;
            }
            if ($response->status >= 200 && $response->status < 300) {
                return $response;
            }

            throw $this->error($response);
        }
    }

    private function error(Response $response): KargonomiException
    {
        $decoded = json_decode($response->body, true);
        $payload = is_array($decoded) ? $decoded : [];
        $message = is_string($payload['message'] ?? null) ? $payload['message'] : 'Kargonomi request failed.';
        $message = str_replace($this->apiToken, '[REDACTED]', $message);
        $requestId = $response->headers['x-request-id'] ?? null;

        if ($response->status === 401 || $response->status === 403) {
            return new AuthenticationException($message, $response->status, $requestId);
        }
        if ($response->status === 404) {
            return new NotFoundException($message, 404, $requestId);
        }
        if ($response->status === 422) {
            /** @var array<string, list<string>> $errors */
            $errors = is_array($payload['errors'] ?? null) ? $payload['errors'] : [];

            return new ValidationException($message, $errors, $requestId);
        }
        if ($response->status === 429) {
            $retry = isset($response->headers['retry-after']) ? (int) $response->headers['retry-after'] : null;

            return new RateLimitException($message, $retry, $requestId);
        }

        return new KargonomiException($message, $response->status, $requestId);
    }
}
