<?php

declare(strict_types=1);

namespace Kargonomi\Model;

final readonly class PriceOffer
{
    public function __construct(
        public int $providerId,
        public string $providerName,
        public ?string $providerSlug,
        public ?string $rawPrice,
        public ?string $amount,
        public bool $available,
    ) {}

    /** @param array<string, mixed> $data */
    public static function fromArray(array $data): self
    {
        $id = $data['id'] ?? null;
        if (!is_int($id) && !(is_string($id) && preg_match('/^-?\d+$/D', $id) === 1)) {
            throw new \UnexpectedValueException('Provider id is required and must be an integer.');
        }
        $raw = isset($data['price']) && is_string($data['price']) ? $data['price'] : null;
        $amount = null;
        if ($raw !== null && preg_match('/^\s*(\d+(?:[.,]\d+)?)/u', $raw, $matches) === 1) {
            $amount = str_replace(',', '.', $matches[1]);
        }

        return new self(
            (int) $id,
            is_string($data['name'] ?? null) ? $data['name'] : '',
            is_string($data['slug'] ?? null) ? $data['slug'] : null,
            $raw,
            $amount,
            $amount !== null || (int) $id === -1,
        );
    }
}
