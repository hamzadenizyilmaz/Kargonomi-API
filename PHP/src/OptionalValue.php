<?php

declare(strict_types=1);

namespace Kargonomi;

final readonly class OptionalValue
{
    private function __construct(public bool $isPresent, public mixed $value) {}

    public static function absent(): self
    {
        return new self(false, null);
    }

    public static function null(): self
    {
        return new self(true, null);
    }

    public static function of(mixed $value): self
    {
        return new self(true, $value);
    }
}
