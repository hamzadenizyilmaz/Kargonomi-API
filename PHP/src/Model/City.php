<?php

declare(strict_types=1);

namespace Kargonomi\Model;

final readonly class City
{
    public function __construct(public int $id, public int $stateId, public string $name) {}
}
