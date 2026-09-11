<?php

declare(strict_types=1);

namespace Kargonomi\Model;

final readonly class Shipment
{
    /**
     * @param list<ShipmentPackage> $packages
     * @param array<string, mixed> $extra
     */
    public function __construct(
        public int $id,
        public ShipmentStatus $status,
        public ?string $statusLabel,
        public ?string $type,
        public ?int $packageCount,
        public ?string $estimatedPrice,
        public ?string $realPrice,
        public ?string $extraShippingPrice,
        public ?string $buyerName,
        public ?string $shippingWebserviceOrderId,
        public ?string $shippingWebserviceBarcode,
        public ?string $shippingWebserviceTrackingCode,
        public ?string $shippingProviderName,
        public ?string $shippingProviderSlug,
        public ?string $barcodeOfOrderId,
        public ?\DateTimeImmutable $shippingWebserviceCreatedAt,
        public ?string $ecommerceProviderOrderNo,
        public ?string $ecommerceProvider,
        public ?\DateTimeImmutable $deliveryDateToShipmentOffice,
        public ?\DateTimeImmutable $shippingProviderCustomerDeliveryDate,
        public ?\DateTimeImmutable $createdAt,
        public ?\DateTimeImmutable $updatedAt,
        public ?ShipmentParty $sender,
        public ?ShipmentParty $buyer,
        public ?ShipmentPricing $pricing,
        public ?ShipmentWarehouse $warehouse,
        public array $packages,
        public array $extra,
    ) {}

    /** @param array<string, mixed> $data */
    public static function fromArray(array $data): self
    {
        $known = array_fill_keys([
            'id', 'status', 'status_label', 'type', 'package_count', 'estimated_price', 'real_price',
            'extra_shipping_price', 'buyer_name', 'created_at', 'updated_at', 'shipment_packages',
            'shipping_webservice_order_id', 'shipping_webservice_barcode', 'shipping_webservice_tracking_code',
            'shipping_provider_name', 'shipping_provider_slug', 'barcode_of_order_id', 'shipping_webservice_created_at',
            'ecommerce_provider_order_no', 'ecommerce_provider', 'delivery_date_to_shipment_office',
            'shipping_provider_customer_delivery_date', 'sender', 'buyer', 'pricing', 'warehouse',
        ], true);
        $sender = self::object($data['sender'] ?? null);
        $buyer = self::object($data['buyer'] ?? null);
        $pricing = self::object($data['pricing'] ?? null);
        $warehouse = self::object($data['warehouse'] ?? null);

        return new self(
            self::integer($data, 'id'),
            new ShipmentStatus(self::text($data, 'status')),
            self::nullableText($data['status_label'] ?? null),
            self::nullableText($data['type'] ?? null),
            self::nullableInteger($data['package_count'] ?? null),
            self::decimal($data['estimated_price'] ?? null),
            self::decimal($data['real_price'] ?? null),
            self::decimal($data['extra_shipping_price'] ?? null),
            self::nullableText($data['buyer_name'] ?? null),
            self::nullableText($data['shipping_webservice_order_id'] ?? null),
            self::nullableText($data['shipping_webservice_barcode'] ?? null),
            self::nullableText($data['shipping_webservice_tracking_code'] ?? null),
            self::nullableText($data['shipping_provider_name'] ?? null),
            self::nullableText($data['shipping_provider_slug'] ?? null),
            self::nullableText($data['barcode_of_order_id'] ?? null),
            self::date($data['shipping_webservice_created_at'] ?? null),
            self::nullableText($data['ecommerce_provider_order_no'] ?? null),
            self::nullableText($data['ecommerce_provider'] ?? null),
            self::date($data['delivery_date_to_shipment_office'] ?? null),
            self::date($data['shipping_provider_customer_delivery_date'] ?? null),
            self::date($data['created_at'] ?? null),
            self::date($data['updated_at'] ?? null),
            $sender === null ? null : ShipmentParty::fromArray($sender, 'sender'),
            $buyer === null ? null : ShipmentParty::fromArray($buyer, 'buyer'),
            $pricing === null ? null : ShipmentPricing::fromArray($pricing),
            $warehouse === null ? null : ShipmentWarehouse::fromArray($warehouse),
            self::packages($data['shipment_packages'] ?? []),
            array_diff_key($data, $known),
        );
    }

    /** @param array<string, mixed> $data */
    private static function integer(array $data, string $key): int
    {
        if (!isset($data[$key]) || !is_numeric($data[$key])) {
            throw new \UnexpectedValueException("Shipment {$key} must be numeric.");
        }

        return (int) $data[$key];
    }

    /** @param array<string, mixed> $data */
    private static function text(array $data, string $key): string
    {
        if (!isset($data[$key]) || !is_string($data[$key])) {
            throw new \UnexpectedValueException("Shipment {$key} must be a string.");
        }

        return $data[$key];
    }

    private static function nullableText(mixed $value): ?string
    {
        return is_string($value) ? $value : null;
    }

    private static function decimal(mixed $value): ?string
    {
        if ($value === null) {
            return null;
        }
        if (!is_string($value) && !is_int($value) && !is_float($value)) {
            throw new \UnexpectedValueException('Price must be represented by a string or number.');
        }

        return (string) $value;
    }

    private static function nullableInteger(mixed $value): ?int
    {
        if ($value === null) {
            return null;
        }
        if (!is_int($value) && !(is_string($value) && ctype_digit($value))) {
            throw new \UnexpectedValueException('Package count must be an integer.');
        }

        return (int) $value;
    }

    private static function date(mixed $value): ?\DateTimeImmutable
    {
        if (!is_string($value) || $value === '') {
            return null;
        }

        return new \DateTimeImmutable($value);
    }

    /** @return list<ShipmentPackage> */
    private static function packages(mixed $value): array
    {
        if (!is_array($value)) {
            return [];
        }

        $result = [];
        foreach ($value as $item) {
            $normalized = self::object($item);
            if ($normalized === null) {
                continue;
            }
            $result[] = ShipmentPackage::fromArray($normalized);
        }

        return $result;
    }

    /** @return array<string, mixed>|null */
    private static function object(mixed $value): ?array
    {
        if (!is_array($value) || array_is_list($value)) {
            return null;
        }
        $result = [];
        foreach ($value as $key => $field) {
            if (is_string($key)) {
                $result[$key] = $field;
            }
        }

        return $result;
    }
}
