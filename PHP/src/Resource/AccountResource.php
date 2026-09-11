<?php

declare(strict_types=1);

namespace Kargonomi\Resource;

use Kargonomi\KargonomiClient;
use Kargonomi\Model\Credit;

final readonly class AccountResource
{
    public function __construct(private KargonomiClient $client) {}

    public function credit(): Credit
    {
        $payload = Decoder::object($this->client->requestJson('GET', '/user/credit'));
        $value = $payload['credit'] ?? null;
        if (!is_string($value) && !is_int($value) && !is_float($value)) {
            throw new \UnexpectedValueException('Credit must be represented by a string or number.');
        }

        return new Credit((string) $value);
    }
}
