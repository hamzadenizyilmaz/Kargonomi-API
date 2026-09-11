<?php

declare(strict_types=1);

namespace Kargonomi\Http;

final readonly class Response
{
    /** @param array<string, string> $headers */
    public function __construct(
        public int $status,
        public array $headers,
        public string $body,
    ) {}

    public static function json(int $status, string $body): self
    {
        return new self($status, ['content-type' => 'application/json'], $body);
    }
}
