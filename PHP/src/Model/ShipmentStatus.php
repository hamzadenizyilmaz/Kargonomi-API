<?php

declare(strict_types=1);

namespace Kargonomi\Model;

final readonly class ShipmentStatus
{
    /** @var list<string> */
    private const array KNOWN = [
        'draft', 'ready', 'webservice_order_failed', 'webservice_order_creating',
        'webservice_order_created', 'webservice_checking_shipment', 'webservice_shipment_started',
        'webservice_shipment_delivered', 'webservice_shipment_not_delivered',
        'webservice_shipment_returning', 'webservice_shipment_missing', 'cancelled',
        'request_for_cancellation',
    ];

    public function __construct(public string $value) {}

    public function isKnown(): bool
    {
        return in_array($this->value, self::KNOWN, true);
    }
}
