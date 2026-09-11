<?php

declare(strict_types=1);

namespace Kargonomi\Model;

final readonly class Warehouse
{
    /** @param array<string, mixed> $extra */
    public function __construct(public int $id, public string $name, public array $extra) {}

    /** @param array<string, mixed> $data */
    public static function fromArray(array $data): self
    {
        $id = $data['id'] ?? null;
        if (!is_int($id) && !(is_string($id) && ctype_digit($id))) {
            throw new \UnexpectedValueException('Warehouse id is required and must be an integer.');
        }

        return new self(
            (int) $id,
            is_string($data['name'] ?? null) ? $data['name'] : '',
            array_diff_key($data, ['id' => true, 'name' => true]),
        );
    }
}
