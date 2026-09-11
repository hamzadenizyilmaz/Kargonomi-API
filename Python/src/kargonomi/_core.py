from __future__ import annotations

import json
from typing import Any, NoReturn

from .errors import (
    AuthenticationError,
    KargonomiError,
    NotFoundError,
    RateLimitError,
    UnexpectedResponseError,
    ValidationError,
    validation_errors,
)
from .transport import Request, Response


def build_request(
    method: str,
    path: str,
    *,
    api_token: str,
    base_url: str,
    timeout: float,
    params: dict[str, int | str] | None = None,
    json_body: dict[str, Any] | None = None,
    form: dict[str, str] | None = None,
) -> Request:
    return Request(
        method=method.upper(),
        base_url=base_url,
        path="/" + path.lstrip("/"),
        headers={"Authorization": f"Bearer {api_token}"},
        params=params or {},
        json=json_body,
        form=form,
        timeout=timeout,
    )


def json_response(response: Response) -> Any:
    if not response.body:
        raise UnexpectedResponseError("Kargonomi returned an empty JSON response.")
    try:
        return json.loads(response.body)
    except (UnicodeDecodeError, json.JSONDecodeError) as exception:
        raise UnexpectedResponseError("Kargonomi returned invalid JSON.") from exception


def raise_for_error(response: Response, api_token: str) -> NoReturn:
    try:
        decoded = json.loads(response.body) if response.body else {}
    except (UnicodeDecodeError, json.JSONDecodeError):
        decoded = {}
    payload = decoded if isinstance(decoded, dict) else {}
    raw_message = payload.get("message")
    message = raw_message if isinstance(raw_message, str) else "Kargonomi request failed."
    message = message.replace(api_token, "[REDACTED]")
    request_id = response.headers.get("x-request-id")
    if response.status in (401, 403):
        raise AuthenticationError(message, status_code=response.status, request_id=request_id)
    if response.status == 404:
        raise NotFoundError(message, status_code=404, request_id=request_id)
    if response.status == 422:
        raise ValidationError(message, validation_errors(payload.get("errors")), request_id=request_id)
    if response.status == 429:
        raw_retry = response.headers.get("retry-after")
        retry_after = int(raw_retry) if raw_retry and raw_retry.isdigit() else None
        raise RateLimitError(message, retry_after, request_id=request_id)
    raise KargonomiError(message, status_code=response.status, request_id=request_id)
