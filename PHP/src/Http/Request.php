<?php

declare(strict_types=1);

namespace Kargonomi\Http;

final readonly class Request
{
    /**
     * @param array<string, string> $headers
     * @param array<string, int|string> $query
     * @param array<string, mixed>|null $json
     * @param array<string, string>|null $form
     */
    public function __construct(
        public string $method,
        public string $baseUrl,
        public string $path,
        public array $headers,
        public array $query = [],
        public ?array $json = null,
        public ?array $form = null,
        public string $accept = 'application/json',
        public int $timeoutSeconds = 30,
    ) {}

    public function url(): string
    {
        $query = http_build_query($this->query, '', '&', PHP_QUERY_RFC3986);

        return rtrim($this->baseUrl, '/') . $this->path . ($query === '' ? '' : '?' . $query);
    }
}
