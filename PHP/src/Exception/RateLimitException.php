<?php

declare(strict_types=1);

namespace Kargonomi\Exception;

final class RateLimitException extends KargonomiException
{
    public function __construct(string $message, public readonly ?int $retryAfterSeconds, ?string $requestId = null)
    {
        parent::__construct($message, 429, $requestId);
    }
}
