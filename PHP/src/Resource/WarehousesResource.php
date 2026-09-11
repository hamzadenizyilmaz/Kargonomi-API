<?php

declare(strict_types=1);

namespace Kargonomi\Resource;

use Kargonomi\KargonomiClient;
use Kargonomi\Model\Warehouse;

final readonly class WarehousesResource
{
    public function __construct(private KargonomiClient $client) {}

    /** @param array<string, mixed> $warehouse */
    public function create(array $warehouse): Warehouse
    {
        $name = $warehouse['name'] ?? null;
        $contactName = $warehouse['contact_name'] ?? null;
        $address = $warehouse['address'] ?? null;
        if (!is_string($name) || strlen(trim($name)) < 2 || strlen($name) > 255) {
            throw new \InvalidArgumentException('Warehouse name must be 2-255 characters.');
        }
        if (!is_bool($warehouse['is_main'] ?? null) || !is_string($contactName) || trim($contactName) === '') {
            throw new \InvalidArgumentException('Warehouse is_main and contact_name are required.');
        }
        if (!is_string($warehouse['contact_phone'] ?? null) || preg_match('/^\d{10}$/D', $warehouse['contact_phone']) !== 1) {
            throw new \InvalidArgumentException('Warehouse contact_phone must contain 10 digits.');
        }
        if (!is_string($address) || strlen(trim($address)) < 10 || strlen($address) > 512) {
            throw new \InvalidArgumentException('Warehouse address must be 10-512 characters.');
        }
        foreach (['state_id', 'city_id'] as $field) {
            if (!is_int($warehouse[$field] ?? null) || $warehouse[$field] < 1) {
                throw new \InvalidArgumentException("Warehouse {$field} must be a positive integer.");
            }
        }
        if (!is_string($warehouse['tax_number'] ?? null) || preg_match('/^\d{10,11}$/D', $warehouse['tax_number']) !== 1) {
            throw new \InvalidArgumentException('Warehouse tax_number must contain 10-11 digits.');
        }

        return Warehouse::fromArray(Decoder::object($this->client->requestJson(
            'POST',
            '/warehouses',
            json: ['warehouse' => $warehouse],
        )));
    }
}
