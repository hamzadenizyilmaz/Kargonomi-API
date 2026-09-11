from __future__ import annotations

import json
from importlib.resources import files
from typing import cast


def configured_base_url() -> str:
    document = json.loads(files("kargonomi").joinpath("appsettings.json").read_text(encoding="utf-8"))
    base_url = cast(dict[str, object], cast(dict[str, object], document)["Kargonomi"])["BaseUrl"]
    if not isinstance(base_url, str) or not base_url:
        raise RuntimeError("Kargonomi:BaseUrl is missing from appsettings.json.")
    return base_url
