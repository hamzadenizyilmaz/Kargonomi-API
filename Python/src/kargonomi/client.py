from __future__ import annotations

import asyncio
import time
from types import TracebackType
from typing import Any, Self

from ._core import build_request, json_response, raise_for_error
from ._settings import configured_base_url
from .async_resources import (
    AsyncAccountResource,
    AsyncBarcodesResource,
    AsyncLocationsResource,
    AsyncPricingResource,
    AsyncShipmentsResource,
    AsyncWarehousesResource,
    AsyncWebhooksResource,
)
from .errors import NetworkError
from .resources import (
    AccountResource,
    BarcodesResource,
    LocationsResource,
    PricingResource,
    ShipmentsResource,
    WarehousesResource,
    WebhooksResource,
)
from .transport import AsyncHttpxTransport, AsyncTransport, HttpxTransport, Response, SyncTransport


def validate_options(api_token: str, base_url: str, timeout: float, max_retries: int) -> None:
    if not api_token.strip():
        raise ValueError("api_token must not be empty.")
    if not base_url.lower().startswith("https://"):
        raise ValueError("base_url must use HTTPS.")
    if timeout <= 0 or max_retries < 0 or max_retries > 5:
        raise ValueError("Invalid timeout or retry configuration.")


def should_retry(method: str, response: Response, attempt: int, max_retries: int) -> bool:
    return method == "GET" and attempt < max_retries and (response.status == 429 or response.status >= 500)


class KargonomiClient:
    """Synchronous Kargonomi API client."""

    def __init__(
        self,
        api_token: str,
        *,
        base_url: str | None = None,
        timeout: float = 30.0,
        max_retries: int = 2,
        transport: SyncTransport | None = None,
    ) -> None:
        resolved_base_url = base_url or configured_base_url()
        validate_options(api_token, resolved_base_url, timeout, max_retries)
        self._api_token = api_token
        self._base_url = resolved_base_url
        self._timeout = timeout
        self._max_retries = max_retries
        self._transport = transport or HttpxTransport()
        self.shipments = ShipmentsResource(self)
        self.pricing = PricingResource(self)
        self.account = AccountResource(self)
        self.locations = LocationsResource(self)
        self.barcodes = BarcodesResource(self)
        self.webhooks = WebhooksResource(self)
        self.warehouses = WarehousesResource(self)

    def request_json(
        self,
        method: str,
        path: str,
        *,
        params: dict[str, int | str] | None = None,
        json_body: dict[str, Any] | None = None,
        form: dict[str, str] | None = None,
    ) -> Any:
        return json_response(self._send(method, path, params=params, json_body=json_body, form=form))

    def request_void(
        self,
        method: str,
        path: str,
        *,
        json_body: dict[str, Any] | None = None,
        form: dict[str, str] | None = None,
    ) -> None:
        self._send(method, path, json_body=json_body, form=form)

    def _send(
        self,
        method: str,
        path: str,
        *,
        params: dict[str, int | str] | None = None,
        json_body: dict[str, Any] | None = None,
        form: dict[str, str] | None = None,
    ) -> Response:
        request = build_request(
            method,
            path,
            api_token=self._api_token,
            base_url=self._base_url,
            timeout=self._timeout,
            params=params,
            json_body=json_body,
            form=form,
        )
        attempt = 0
        while True:
            try:
                response = self._transport.send(request)
            except NetworkError:
                if request.method != "GET" or attempt >= self._max_retries:
                    raise
                attempt += 1
                time.sleep(0.1 * (2 ** (attempt - 1)))
                continue
            if should_retry(request.method, response, attempt, self._max_retries):
                attempt += 1
                time.sleep(0.1 * (2 ** (attempt - 1)))
                continue
            if 200 <= response.status < 300:
                return response
            raise_for_error(response, self._api_token)

    def close(self) -> None:
        self._transport.close()

    def __enter__(self) -> Self:
        return self

    def __exit__(
        self, exc_type: type[BaseException] | None, exc: BaseException | None, tb: TracebackType | None
    ) -> None:
        self.close()


class AsyncKargonomiClient:
    """Asynchronous Kargonomi API client backed by a real async transport."""

    def __init__(
        self,
        api_token: str,
        *,
        base_url: str | None = None,
        timeout: float = 30.0,
        max_retries: int = 2,
        transport: AsyncTransport | None = None,
    ) -> None:
        resolved_base_url = base_url or configured_base_url()
        validate_options(api_token, resolved_base_url, timeout, max_retries)
        self._api_token = api_token
        self._base_url = resolved_base_url
        self._timeout = timeout
        self._max_retries = max_retries
        self._transport = transport or AsyncHttpxTransport()
        self.shipments = AsyncShipmentsResource(self)
        self.pricing = AsyncPricingResource(self)
        self.account = AsyncAccountResource(self)
        self.locations = AsyncLocationsResource(self)
        self.barcodes = AsyncBarcodesResource(self)
        self.webhooks = AsyncWebhooksResource(self)
        self.warehouses = AsyncWarehousesResource(self)

    async def request_json(
        self,
        method: str,
        path: str,
        *,
        params: dict[str, int | str] | None = None,
        json_body: dict[str, Any] | None = None,
        form: dict[str, str] | None = None,
    ) -> Any:
        return json_response(await self._send(method, path, params=params, json_body=json_body, form=form))

    async def request_void(
        self,
        method: str,
        path: str,
        *,
        json_body: dict[str, Any] | None = None,
        form: dict[str, str] | None = None,
    ) -> None:
        await self._send(method, path, json_body=json_body, form=form)

    async def _send(
        self,
        method: str,
        path: str,
        *,
        params: dict[str, int | str] | None = None,
        json_body: dict[str, Any] | None = None,
        form: dict[str, str] | None = None,
    ) -> Response:
        request = build_request(
            method,
            path,
            api_token=self._api_token,
            base_url=self._base_url,
            timeout=self._timeout,
            params=params,
            json_body=json_body,
            form=form,
        )
        attempt = 0
        while True:
            try:
                response = await self._transport.send(request)
            except NetworkError:
                if request.method != "GET" or attempt >= self._max_retries:
                    raise
                attempt += 1
                await asyncio.sleep(0.1 * (2 ** (attempt - 1)))
                continue
            if should_retry(request.method, response, attempt, self._max_retries):
                attempt += 1
                await asyncio.sleep(0.1 * (2 ** (attempt - 1)))
                continue
            if 200 <= response.status < 300:
                return response
            raise_for_error(response, self._api_token)

    async def aclose(self) -> None:
        await self._transport.aclose()

    async def __aenter__(self) -> Self:
        return self

    async def __aexit__(
        self, exc_type: type[BaseException] | None, exc: BaseException | None, tb: TracebackType | None
    ) -> None:
        await self.aclose()
