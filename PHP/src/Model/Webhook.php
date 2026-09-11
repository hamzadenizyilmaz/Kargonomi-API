<?php

declare(strict_types=1);

namespace Kargonomi\Model;

final readonly class Webhook
{
    /** @param array<string, mixed> $extra */
    public function __construct(
        public int $id,
        public ?string $name,
        public string $url,
        public string $eventType,
        public bool $isActive,
        public array $extra,
    ) {}

    /** @param array<string, mixed> $data */
    public static function fromArray(array $data): self
    {
        $id = $data['id'] ?? null;
        if (!is_int($id) && !(is_string($id) && ctype_digit($id))) {
            throw new \UnexpectedValueException('Webhook id is required and must be an integer.');
        }

        return new self(
            (int) $id,
            is_string($data['name'] ?? null) ? $data['name'] : null,
            is_string($data['url'] ?? null) ? $data['url'] : '',
            is_string($data['event_type'] ?? null) ? $data['event_type'] : '',
            (bool) ($data['is_active'] ?? true),
            array_diff_key($data, array_fill_keys(['id', 'name', 'url', 'event_type', 'is_active'], true)),
        );
    }
}
