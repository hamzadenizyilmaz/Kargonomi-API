<?php

declare(strict_types=1);

namespace Kargonomi\Resource;

use Kargonomi\KargonomiClient;
use Kargonomi\Model\Shipment;
use Kargonomi\Model\ShipmentPage;
use Kargonomi\OptionalValue;

final readonly class ShipmentsResource
{
    public function __construct(private KargonomiClient $client) {}

    public function list(int $page = 1): ShipmentPage
    {
        self::positive($page, 'page');
        $payload = $this->client->requestJson('GET', '/shipments', ['page' => $page]);
        $items = array_map(Shipment::fromArray(...), Decoder::collection($payload));
        $meta = is_array($payload['meta'] ?? null) ? $payload['meta'] : [];
        $links = is_array($payload['links'] ?? null) ? $payload['links'] : [];

        return new ShipmentPage(
            $items,
            Decoder::integer($meta['current_page'] ?? $page, 'current_page'),
            Decoder::integer($meta['last_page'] ?? $page, 'last_page'),
            Decoder::integer($meta['per_page'] ?? count($items), 'per_page'),
            Decoder::integer($meta['total'] ?? count($items), 'total'),
            is_string($links['next'] ?? null) ? $links['next'] : null,
            is_string($links['prev'] ?? null) ? $links['prev'] : null,
        );
    }

    public function get(int $id): Shipment
    {
        self::positive($id);

        return Shipment::fromArray(Decoder::object($this->client->requestJson('GET', "/shipments/{$id}")));
    }

    /** @param array<string, mixed> $shipment */
    public function create(array $shipment): Shipment
    {
        self::validateWrite($shipment);

        return Shipment::fromArray(Decoder::object($this->client->requestJson('POST', '/shipments', json: ['shipment' => $shipment])));
    }

    /** @param array<string, mixed> $shipment */
    public function update(int $id, array $shipment): Shipment
    {
        self::positive($id);
        self::validateWrite($shipment);

        return Shipment::fromArray(Decoder::object($this->client->requestJson('PUT', "/shipments/{$id}", json: ['shipment' => $shipment])));
    }

    /** @param array<string, mixed> $fields */
    public function patch(int $id, array $fields): Shipment
    {
        self::positive($id);
        $wire = [];
        foreach ($fields as $name => $optional) {
            if (!$optional instanceof OptionalValue) {
                throw new \InvalidArgumentException('PATCH values must be OptionalValue instances.');
            }
            if ($optional->isPresent) {
                $wire[$name] = $optional->value;
            }
        }
        if ($wire === []) {
            throw new \InvalidArgumentException('PATCH must contain at least one present field.');
        }

        return Shipment::fromArray(Decoder::object($this->client->requestJson('PATCH', "/shipments/{$id}", json: ['shipment' => $wire])));
    }

    public function delete(int $id): void
    {
        self::positive($id);
        $this->client->requestVoid('DELETE', "/shipments/{$id}");
    }

    public function cancel(int $id): void
    {
        self::positive($id);
        $this->client->requestVoid('POST', '/shipments/cancel', ['shipment_id' => $id]);
    }

    /** @return \Generator<int, Shipment> */
    public function iterAll(): \Generator
    {
        $pageNumber = 1;
        $visited = [];
        while (!isset($visited[$pageNumber])) {
            $visited[$pageNumber] = true;
            $page = $this->list($pageNumber);
            yield from $page->items;
            if ($page->currentPage >= $page->lastPage || $page->next === null) {
                return;
            }
            $pageNumber = $page->currentPage + 1;
        }
    }

    /** @param array<string, mixed> $shipment */
    private static function validateWrite(array $shipment): void
    {
        $errors = [];
        $buyerName = $shipment['buyer_name'] ?? null;
        if (!is_string($buyerName) || strlen(trim($buyerName)) < 5 || count(preg_split('/\s+/u', trim($buyerName)) ?: []) < 2) {
            $errors[] = 'buyer_name must contain at least two words and five characters.';
        }
        if (!is_string($shipment['buyer_phone'] ?? null) || preg_match('/^\d{10}$/D', $shipment['buyer_phone']) !== 1) {
            $errors[] = 'buyer_phone must contain exactly 10 digits.';
        }
        $buyerAddress = $shipment['buyer_address'] ?? null;
        if (!is_string($buyerAddress) || strlen(trim($buyerAddress)) < 10 || strlen($buyerAddress) > 512) {
            $errors[] = 'buyer_address must contain 10-512 characters.';
        }
        foreach (['buyer_state_id', 'buyer_city_id'] as $field) {
            if (!is_int($shipment[$field] ?? null) || $shipment[$field] < 1) {
                $errors[] = "{$field} must be a positive integer.";
            }
        }
        $packages = $shipment['packages'] ?? null;
        if (!is_array($packages) || $packages === [] || array_filter($packages, static fn(mixed $package): bool => !is_array($package) || !is_numeric($package['desi'] ?? null) || (float) $package['desi'] <= 0) !== []) {
            $errors[] = 'packages must contain at least one package with positive desi.';
        }
        $manualFields = ['sender_name', 'sender_email', 'sender_tax_number', 'sender_tax_place', 'sender_phone', 'sender_address', 'sender_state_id', 'sender_city_id'];
        $warehouse = $shipment['warehouse_id'] ?? null;
        if ($warehouse !== null && (!is_int($warehouse) || $warehouse < 1)) {
            $errors[] = 'warehouse_id must be a positive integer.';
        }
        if ($warehouse !== null && array_filter($manualFields, static fn(string $field): bool => array_key_exists($field, $shipment)) !== []) {
            $errors[] = 'warehouse_id cannot be combined with manual sender fields.';
        }
        if ($warehouse === null) {
            foreach (['sender_name', 'sender_phone', 'sender_address', 'sender_state_id', 'sender_city_id'] as $field) {
                if (!array_key_exists($field, $shipment)) {
                    $errors[] = "{$field} is required without warehouse_id.";
                }
            }
        }
        if ($errors !== []) {
            throw new \InvalidArgumentException(implode(' ', $errors));
        }
    }

    private static function positive(int $value, string $name = 'id'): void
    {
        if ($value < 1) {
            throw new \InvalidArgumentException("{$name} must be positive.");
        }
    }
}
