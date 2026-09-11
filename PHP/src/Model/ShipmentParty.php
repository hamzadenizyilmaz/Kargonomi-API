<?php

declare(strict_types=1);

namespace Kargonomi\Model;

final readonly class ShipmentParty
{
    public function __construct(
        public ?string $name,
        public ?string $email,
        public ?string $phone,
        public ?string $phone2,
        public ?string $taxNumber,
        public ?string $taxPlace,
        public ?string $address,
        public ?string $state,
        public ?string $city,
    ) {}

    /** @param array<string, mixed> $data */
    public static function fromArray(array $data, string $prefix): self
    {
        $value = static fn(string $field): ?string => is_string($data[$prefix . '_' . $field] ?? null)
            ? $data[$prefix . '_' . $field]
            : null;

        return new self($value('name'), $value('email'), $value('phone'), $value('phone2'), $value('tax_number'), $value('tax_place'), $value('address'), $value('state'), $value('city'));
    }
}
