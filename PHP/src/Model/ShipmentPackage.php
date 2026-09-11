<?php

declare(strict_types=1);

namespace Kargonomi\Model;

final readonly class ShipmentPackage
{
    /** @param array<string, mixed> $extra */
    public function __construct(
        public string $desi,
        public ?string $barcode,
        public ?string $content,
        public ?string $realDesi,
        public array $extra,
    ) {}

    /** @param array<string, mixed> $data */
    public static function fromArray(array $data): self
    {
        if (!is_string($data['desi'] ?? null)) {
            throw new \UnexpectedValueException('Package desi must be a string.');
        }
        $nullable = static fn(mixed $value): ?string => is_string($value) ? $value : null;

        return new self(
            $data['desi'],
            $nullable($data['barcode'] ?? null),
            $nullable($data['content'] ?? null),
            $nullable($data['real_desi'] ?? null),
            array_diff_key($data, ['desi' => true, 'barcode' => true, 'content' => true, 'real_desi' => true]),
        );
    }
}
