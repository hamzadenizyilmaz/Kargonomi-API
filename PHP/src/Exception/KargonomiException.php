<?php

declare(strict_types=1);

namespace Kargonomi\Exception;

class KargonomiException extends \RuntimeException
{
    public function __construct(
        string $message,
        public readonly ?int $statusCode = null,
        public readonly ?string $requestId = null,
    ) {
        parent::__construct($message);
    }
}
