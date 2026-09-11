<?php

declare(strict_types=1);

namespace Kargonomi\Resource;

use Kargonomi\KargonomiClient;
use Kargonomi\Model\Webhook;

final readonly class WebhooksResource
{
    public function __construct(private KargonomiClient $client) {}

    /** @return list<Webhook> */
    public function list(): array
    {
        return array_map(Webhook::fromArray(...), Decoder::collection($this->client->requestJson('GET', '/webhooks')));
    }

    public function get(int $id): Webhook
    {
        self::id($id);

        return Webhook::fromArray(Decoder::object($this->client->requestJson('GET', "/webhooks/{$id}")));
    }

    /** @param list<string> $eventTypes */
    public function create(string $url, array $eventTypes, ?string $name = null, bool $isActive = true): Webhook
    {
        $wire = self::wire($url, $eventTypes, $name, $isActive);

        return Webhook::fromArray(Decoder::object($this->client->requestJson('POST', '/webhooks', json: $wire)));
    }

    /** @param list<string> $eventTypes */
    public function update(int $id, string $url, array $eventTypes, ?string $name = null, bool $isActive = true): void
    {
        self::id($id);
        $this->client->requestVoid('PUT', "/webhooks/{$id}", self::wire($url, $eventTypes, $name, $isActive));
    }

    public function delete(int $id): void
    {
        self::id($id);
        $this->client->requestVoid('DELETE', "/webhooks/{$id}");
    }

    /**
     * @param list<string> $eventTypes
     * @return array<string, mixed>
     */
    private static function wire(string $url, array $eventTypes, ?string $name, bool $isActive): array
    {
        $host = parse_url($url, PHP_URL_HOST);
        $isIp = is_string($host) && filter_var($host, FILTER_VALIDATE_IP) !== false;
        $isPublicIp = !$isIp || filter_var($host, FILTER_VALIDATE_IP, FILTER_FLAG_NO_PRIV_RANGE | FILTER_FLAG_NO_RES_RANGE) !== false;
        $normalizedHost = is_string($host) ? strtolower($host) : '';
        $isLocalName = $normalizedHost === 'localhost' || str_ends_with($normalizedHost, '.localhost') || str_ends_with($normalizedHost, '.local') || str_ends_with($normalizedHost, '.internal') || !str_contains($normalizedHost, '.');
        if (filter_var($url, FILTER_VALIDATE_URL) === false || !str_starts_with(strtolower($url), 'https://') || parse_url($url, PHP_URL_USER) !== null || !$isPublicIp || $isLocalName) {
            throw new \InvalidArgumentException('Webhook URL must target a public HTTPS host without user information.');
        }
        if ($eventTypes === [] || trim($eventTypes[0]) === '') {
            throw new \InvalidArgumentException('At least one webhook event type is required.');
        }

        return array_filter([
            'name' => $name,
            'url' => $url,
            'event_type' => $eventTypes[0],
            'is_active' => $isActive,
        ], static fn(mixed $value): bool => $value !== null);
    }

    private static function id(int $id): void
    {
        if ($id < 1) {
            throw new \InvalidArgumentException('id must be positive.');
        }
    }
}
