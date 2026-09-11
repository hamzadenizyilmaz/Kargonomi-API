<?php

declare(strict_types=1);

namespace Kargonomi\Exception;

final class ValidationException extends KargonomiException
{
    /** @param array<string, list<string>> $errors */
    public function __construct(string $message, public readonly array $errors, ?string $requestId = null)
    {
        parent::__construct($message, 422, $requestId);
    }
}
