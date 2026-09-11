from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any, Protocol
from urllib.parse import urlencode

import httpx

from .errors import NetworkError, TimeoutError


@dataclass(frozen=True, slots=True)
class Request:
    method: str
    base_url: str
    path: str
    headers: dict[str, str]
    params: dict[str, int | str] = field(default_factory=dict)
    json: dict[str, Any] | None = None
    form: dict[str, str] | None = None
    accept: str = "application/json"
    timeout: float = 30.0

    @property
    def url(self) -> str:
        query = urlencode(self.params)
        return f"{self.base_url.rstrip('/')}{self.path}{'?' + query if query else ''}"


@dataclass(frozen=True, slots=True)
class Response:
    status: int
    headers: dict[str, str]
    body: bytes


class SyncTransport(Protocol):
    def send(self, request: Request) -> Response: ...

    def close(self) -> None: ...


class AsyncTransport(Protocol):
    async def send(self, request: Request) -> Response: ...

    async def aclose(self) -> None: ...


class HttpxTransport:
    def __init__(self) -> None:
        self._client = httpx.Client(follow_redirects=False)

    def send(self, request: Request) -> Response:
        try:
            result = self._client.request(
                request.method,
                request.url,
                headers={**request.headers, "Accept": request.accept},
                json=request.json,
                files={key: (None, value) for key, value in request.form.items()} if request.form else None,
                timeout=request.timeout,
            )
        except httpx.TimeoutException as exception:
            raise TimeoutError("The Kargonomi request timed out.") from exception
        except httpx.TransportError as exception:
            raise NetworkError(f"The Kargonomi request failed: {exception}") from exception
        return Response(result.status_code, dict(result.headers), result.content)

    def close(self) -> None:
        self._client.close()


class AsyncHttpxTransport:
    def __init__(self) -> None:
        self._client = httpx.AsyncClient(follow_redirects=False)

    async def send(self, request: Request) -> Response:
        try:
            result = await self._client.request(
                request.method,
                request.url,
                headers={**request.headers, "Accept": request.accept},
                json=request.json,
                files={key: (None, value) for key, value in request.form.items()} if request.form else None,
                timeout=request.timeout,
            )
        except httpx.TimeoutException as exception:
            raise TimeoutError("The Kargonomi request timed out.") from exception
        except httpx.TransportError as exception:
            raise NetworkError(f"The Kargonomi request failed: {exception}") from exception
        return Response(result.status_code, dict(result.headers), result.content)

    async def aclose(self) -> None:
        await self._client.aclose()
