<?php

declare(strict_types=1);

namespace Kargonomi\Resource;

use Kargonomi\KargonomiClient;
use Kargonomi\Model\City;
use Kargonomi\Model\State;

final readonly class LocationsResource
{
    public function __construct(private KargonomiClient $client) {}

    /** @return list<State> */
    public function states(?int $countryId = null): array
    {
        if ($countryId !== null && $countryId < 1) {
            throw new \InvalidArgumentException('countryId must be positive.');
        }
        $path = $countryId === null ? '/states' : "/states/{$countryId}";

        return array_map(
            static fn(array $item): State => new State(
                Decoder::integer($item['id'] ?? null, 'state.id'),
                Decoder::text($item['name'] ?? null, 'state.name'),
            ),
            Decoder::collection($this->client->requestJson('GET', $path)),
        );
    }

    /** @return list<City> */
    public function cities(int $stateId): array
    {
        if ($stateId < 1) {
            throw new \InvalidArgumentException('stateId must be positive.');
        }

        return array_map(
            static fn(array $item): City => new City(
                Decoder::integer($item['id'] ?? null, 'city.id'),
                Decoder::integer($item['state_id'] ?? $stateId, 'city.state_id'),
                Decoder::text($item['name'] ?? null, 'city.name'),
            ),
            Decoder::collection($this->client->requestJson('GET', "/cities/{$stateId}")),
        );
    }
}
