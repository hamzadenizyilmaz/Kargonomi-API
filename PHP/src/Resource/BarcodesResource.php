<?php

declare(strict_types=1);

namespace Kargonomi\Resource;

use Kargonomi\KargonomiClient;

final readonly class BarcodesResource
{
    private const int MAX_BYTES = 26_214_400;

    public function __construct(private KargonomiClient $client) {}

    public function download(int $shipmentId): string
    {
        if ($shipmentId < 1) {
            throw new \InvalidArgumentException('shipmentId must be positive.');
        }
        $payload = Decoder::object($this->client->requestJson(
            'GET',
            "/shipments/{$shipmentId}/barcode",
            ['format' => 'pdf'],
        ));
        $encoded = $payload['data'] ?? null;
        if (!is_string($encoded)) {
            throw new \UnexpectedValueException('Barcode response has no Base64 data.');
        }
        $bytes = base64_decode($encoded, true);
        if ($bytes === false || strlen($bytes) > self::MAX_BYTES || !str_starts_with($bytes, '%PDF')) {
            throw new \UnexpectedValueException('Kargonomi barcode is invalid PDF content.');
        }

        return $bytes;
    }
}
