<?php

declare(strict_types=1);

namespace Kargonomi\Model;

final readonly class Credit
{
    public function __construct(public string $amount) {}
}
