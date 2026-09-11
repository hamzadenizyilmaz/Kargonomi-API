<?php

declare(strict_types=1);

namespace Kargonomi\Http;

use Kargonomi\Exception\NetworkException;
use Kargonomi\Exception\TimeoutException;

final class CurlTransport implements TransportInterface
{
    public function send(Request $request): Response
    {
        $handle = curl_init();
        if ($handle === false) {
            throw new NetworkException('Unable to initialize the HTTP transport.');
        }

        /** @var array<string, string> $responseHeaders */
        $responseHeaders = [];
        $headers = array_merge($request->headers, ['Accept' => $request->accept]);
        $body = null;
        if ($request->json !== null) {
            $body = json_encode($request->json, JSON_THROW_ON_ERROR | JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
            $headers['Content-Type'] = 'application/json';
        } elseif ($request->form !== null) {
            $body = $request->form;
        }

        $headerLines = [];
        foreach ($headers as $name => $value) {
            $headerLines[] = $name . ': ' . $value;
        }

        $url = $request->url();
        if ($url === '' || $request->method === '') {
            throw new NetworkException('The Kargonomi request URL or method is empty.');
        }
        curl_setopt($handle, CURLOPT_URL, $url);
        curl_setopt($handle, CURLOPT_CUSTOMREQUEST, $request->method);
        curl_setopt($handle, CURLOPT_HTTPHEADER, $headerLines);
        curl_setopt($handle, CURLOPT_RETURNTRANSFER, true);
        curl_setopt($handle, CURLOPT_FOLLOWLOCATION, false);
        curl_setopt($handle, CURLOPT_CONNECTTIMEOUT, min(10, $request->timeoutSeconds));
        curl_setopt($handle, CURLOPT_TIMEOUT, $request->timeoutSeconds);
        curl_setopt($handle, CURLOPT_PROTOCOLS, CURLPROTO_HTTPS);
        curl_setopt($handle, CURLOPT_REDIR_PROTOCOLS, CURLPROTO_HTTPS);
        curl_setopt($handle, CURLOPT_USERAGENT, 'kargonomi-php/0.1.0');
        curl_setopt(
            $handle,
            CURLOPT_HEADERFUNCTION,
            static function (\CurlHandle $unused, string $line) use (&$responseHeaders): int {
                $separator = strpos($line, ':');
                if ($separator !== false) {
                    $name = strtolower(trim(substr($line, 0, $separator)));
                    $responseHeaders[$name] = trim(substr($line, $separator + 1));
                }

                return strlen($line);
            },
        );
        if ($body !== null) {
            curl_setopt($handle, CURLOPT_POSTFIELDS, $body);
        }

        $result = curl_exec($handle);
        if ($result === false) {
            $errorCode = curl_errno($handle);
            $message = curl_error($handle);
            curl_close($handle);
            if ($errorCode === CURLE_OPERATION_TIMEDOUT) {
                throw new TimeoutException('The Kargonomi request timed out.');
            }

            throw new NetworkException('The Kargonomi request failed: ' . $message);
        }

        $status = curl_getinfo($handle, CURLINFO_RESPONSE_CODE);
        curl_close($handle);

        return new Response($status, $responseHeaders, $result === true ? '' : $result);
    }
}
