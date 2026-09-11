<?php

declare(strict_types=1);

namespace Kargonomi\Resource;

use Kargonomi\KargonomiClient;
use Kargonomi\Model\PriceOffer;

final readonly class PricingResource
{
    public function __construct(private KargonomiClient $client) {}

    /** @return list<PriceOffer> */
    public function compare(int $shipmentId): array
    {
        self::shipmentId($shipmentId);
        $payload = $this->client->requestJson('GET', "/shipment-price-comparison/{$shipmentId}");
        $items = $payload['shipping_provider_with_price'] ?? null;
        if (!is_array($items)) {
            throw new \UnexpectedValueException('Price comparison response has no offers.');
        }

        $offers = [];
        foreach ($items as $item) {
            if (is_array($item)) {
                $offers[] = PriceOffer::fromArray(Decoder::object($item));
            }
        }

        return $offers;
    }

    public function confirm(int $shipmentId, int $shippingProviderId): void
    {
        self::shipmentId($shipmentId);
        if ($shippingProviderId === 0 || $shippingProviderId < -1) {
            throw new \InvalidArgumentException('shippingProviderId must be -1 or positive.');
        }
        $this->client->requestVoid('POST', '/confirm-shipping-price', form: [
            'shipment_id' => (string) $shipmentId,
            'shipping_provider_id' => (string) $shippingProviderId,
        ]);
    }

    private static function shipmentId(int $id): void
    {
        if ($id < 1) {
            throw new \InvalidArgumentException('shipmentId must be positive.');
        }
    }
}
