<?php

declare(strict_types=1);

namespace Kargonomi\Model;

final readonly class ShipmentWarehouse
{
    /** @param array<string, mixed> $extra */
    public function __construct(
        public int $id,
        public string $name,
        public bool $isMain,
        public ?string $contactName,
        public ?string $contactPhone,
        public ?string $state,
        public ?string $city,
        public ?string $address,
        public ?string $phone,
        public ?string $taxNumber,
        public ?string $taxPlace,
        public array $extra,
    ) {}

    /** @param array<string, mixed> $data */
    public static function fromArray(array $data): self
    {
        $id = $data['id'] ?? null;
        $isMain = $data['is_main'] ?? null;
        if ((!is_int($id) && !(is_string($id) && ctype_digit($id))) || (!is_bool($isMain) && $isMain !== 0 && $isMain !== 1)) {
            throw new \UnexpectedValueException('Shipment warehouse id or is_main is invalid.');
        }
        if (!is_string($data['name'] ?? null)) {
            throw new \UnexpectedValueException('Shipment warehouse name is required.');
        }
        $nullable = static fn(mixed $value): ?string => is_string($value) ? $value : null;
        $known = array_fill_keys(['id', 'name', 'is_main', 'contact_name', 'contact_phone', 'state', 'city', 'address', 'phone', 'tax_number', 'tax_place'], true);

        return new self(
            (int) $id,
            $data['name'],
            (bool) $isMain,
            $nullable($data['contact_name'] ?? null),
            $nullable($data['contact_phone'] ?? null),
            $nullable($data['state'] ?? null),
            $nullable($data['city'] ?? null),
            $nullable($data['address'] ?? null),
            $nullable($data['phone'] ?? null),
            $nullable($data['tax_number'] ?? null),
            $nullable($data['tax_place'] ?? null),
            array_diff_key($data, $known),
        );
    }
}
