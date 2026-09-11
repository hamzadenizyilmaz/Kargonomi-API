<?php

declare(strict_types=1);

namespace Kargonomi\Model;

final readonly class ShipmentPricing
{
    public function __construct(
        public int $packageCount,
        public ?string $estimatedPrice,
        public ?string $realPrice,
        public ?string $extraShippingPrice,
        public ?string $priceDifference,
    ) {}

    /** @param array<string, mixed> $data */
    public static function fromArray(array $data): self
    {
        $packageCount = $data['package_count'] ?? null;
        if (!is_int($packageCount) && !(is_string($packageCount) && ctype_digit($packageCount))) {
            throw new \UnexpectedValueException('Pricing package_count must be an integer.');
        }
        $decimal = static function (mixed $value): ?string {
            if ($value === null) {
                return null;
            }
            if (!is_string($value) && !is_int($value) && !is_float($value)) {
                throw new \UnexpectedValueException('Pricing value must be decimal-compatible.');
            }

            return (string) $value;
        };

        return new self((int) $packageCount, $decimal($data['estimated_price'] ?? null), $decimal($data['real_price'] ?? null), $decimal($data['extra_shipping_price'] ?? null), $decimal($data['price_diff'] ?? null));
    }
}
