<?php

declare(strict_types=1);

namespace Kargonomi\Model;

final readonly class State
{
    public function __construct(public int $id, public string $name) {}
}
