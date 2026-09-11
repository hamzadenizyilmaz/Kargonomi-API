<?php

declare(strict_types=1);

namespace Kargonomi\Resource;

final class Decoder
{
    public static function integer(mixed $value, string $field): int
    {
        if (!is_int($value) && !(is_string($value) && preg_match('/^-?\d+$/D', $value) === 1)) {
            throw new \UnexpectedValueException("{$field} must be an integer.");
        }

        return (int) $value;
    }

    public static function text(mixed $value, string $field): string
    {
        if (!is_string($value)) {
            throw new \UnexpectedValueException("{$field} must be a string.");
        }

        return $value;
    }

    /**
     * @param array<mixed> $payload
     * @return array<string, mixed>
     */
    public static function object(array $payload): array
    {
        $value = $payload['data'] ?? $payload;
        if (!is_array($value) || array_is_list($value)) {
            throw new \UnexpectedValueException('Expected an object response.');
        }

        $normalized = [];
        foreach ($value as $key => $item) {
            if (is_string($key)) {
                $normalized[$key] = $item;
            }
        }

        return $normalized;
    }

    /**
     * @param array<mixed> $payload
     * @return list<array<string, mixed>>
     */
    public static function collection(array $payload): array
    {
        $value = $payload['data'] ?? $payload;
        if (!is_array($value)) {
            throw new \UnexpectedValueException('Expected a collection response.');
        }

        $result = [];
        foreach ($value as $item) {
            if (!is_array($item) || array_is_list($item)) {
                throw new \UnexpectedValueException('Collection item must be an object.');
            }
            $normalized = [];
            foreach ($item as $key => $field) {
                if (is_string($key)) {
                    $normalized[$key] = $field;
                }
            }
            $result[] = $normalized;
        }

        return $result;
    }
}
