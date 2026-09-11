from __future__ import annotations

import base64
import hashlib
import hmac
from typing import Literal


def verify_webhook_signature(
    raw_body: bytes,
    signature: str,
    secret: str,
    encoding: Literal["hex", "base64"] = "hex",
) -> bool:
    """Verify HMAC-SHA256 against the exact request bytes before parsing JSON."""
    digest = hmac.new(secret.encode(), raw_body, hashlib.sha256).digest()
    expected = digest.hex() if encoding == "hex" else base64.b64encode(digest).decode("ascii")
    return hmac.compare_digest(expected, signature.strip())
