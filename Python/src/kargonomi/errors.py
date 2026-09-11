from __future__ import annotations

from typing import Any


class KargonomiError(RuntimeError):
    """Base exception for provider, protocol, and transport failures."""

    def __init__(
        self, message: str, *, status_code: int | None = None, request_id: str | None = None
    ) -> None:
        super().__init__(message)
        self.status_code = status_code
        self.request_id = request_id


class AuthenticationError(KargonomiError):
    """The provider rejected the API credentials."""


class NotFoundError(KargonomiError):
    """The requested provider resource does not exist."""


class ValidationError(KargonomiError):
    """The provider rejected request fields with HTTP 422."""

    def __init__(self, message: str, errors: dict[str, list[str]], *, request_id: str | None = None) -> None:
        super().__init__(message, status_code=422, request_id=request_id)
        self.errors = errors


class RateLimitError(KargonomiError):
    """The provider rate limit was reached."""

    def __init__(self, message: str, retry_after: int | None, *, request_id: str | None = None) -> None:
        super().__init__(message, status_code=429, request_id=request_id)
        self.retry_after = retry_after


class NetworkError(KargonomiError):
    """The HTTP request could not reach the provider."""


class TimeoutError(NetworkError):
    """The HTTP request exceeded its configured deadline."""


class UnexpectedResponseError(KargonomiError):
    """The provider response was successful but violated the expected contract."""


def validation_errors(value: Any) -> dict[str, list[str]]:
    if not isinstance(value, dict):
        return {}
    result: dict[str, list[str]] = {}
    for key, messages in value.items():
        if (
            isinstance(key, str)
            and isinstance(messages, list)
            and all(isinstance(item, str) for item in messages)
        ):
            result[key] = messages
    return result
