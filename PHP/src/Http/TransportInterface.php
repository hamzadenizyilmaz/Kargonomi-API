<?php

declare(strict_types=1);

namespace Kargonomi\Http;

interface TransportInterface
{
    public function send(Request $request): Response;
}
