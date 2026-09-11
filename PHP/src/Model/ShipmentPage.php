<?php

declare(strict_types=1);

namespace Kargonomi\Model;

final readonly class ShipmentPage
{
    /** @param list<Shipment> $items */
    public function __construct(
        public array $items,
        public int $currentPage,
        public int $lastPage,
        public int $perPage,
        public int $total,
        public ?string $next,
        public ?string $previous,
    ) {}
}
