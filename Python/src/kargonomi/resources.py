from __future__ import annotations

import base64
import ipaddress
import re
from collections.abc import Generator, Mapping
from decimal import Decimal, InvalidOperation
from typing import Any, Protocol
from urllib.parse import urlparse

from .errors import UnexpectedResponseError
from .models import (
    BarcodeDocument,
    City,
    Credit,
    PriceComparison,
    PriceOffer,
    Shipment,
    ShipmentPage,
    State,
    Warehouse,
    Webhook,
    decimal,
    envelope_list,
    envelope_object,
    integer,
    nullable_text,
    object_value,
    shipment,
    text,
)


class UnsetType:
    __slots__ = ()

    def __repr__(self) -> str:
        return "UNSET"


UNSET = UnsetType()


def shipment_write(value: Mapping[str, Any]) -> dict[str, Any]:
    wire = dict(value)
    errors: list[str] = []
    buyer_name = wire.get("buyer_name")
    if not isinstance(buyer_name, str) or len(buyer_name.strip()) < 5 or len(buyer_name.split()) < 2:
        errors.append("buyer_name must contain at least two words and five characters.")
    buyer_phone = wire.get("buyer_phone")
    if not isinstance(buyer_phone, str) or len(buyer_phone) != 10 or not buyer_phone.isdecimal():
        errors.append("buyer_phone must contain exactly 10 digits.")
    buyer_address = wire.get("buyer_address")
    if not isinstance(buyer_address, str) or not 10 <= len(buyer_address.strip()) <= 512:
        errors.append("buyer_address must contain 10-512 characters.")
    for field in ("buyer_state_id", "buyer_city_id"):
        field_value = wire.get(field)
        if isinstance(field_value, bool) or not isinstance(field_value, int) or field_value < 1:
            errors.append(f"{field} must be a positive integer.")
    packages = wire.get("packages")
    if not isinstance(packages, list) or not packages:
        errors.append("packages must contain at least one package with positive desi.")
    else:
        try:
            if any(
                not isinstance(package, Mapping)
                or not Decimal(str(package.get("desi"))).is_finite()
                or Decimal(str(package.get("desi"))) <= 0
                for package in packages
            ):
                errors.append("packages must contain at least one package with positive desi.")
        except (InvalidOperation, ValueError):
            errors.append("packages must contain at least one package with positive desi.")
    manual_fields = (
        "sender_name",
        "sender_email",
        "sender_tax_number",
        "sender_tax_place",
        "sender_phone",
        "sender_address",
        "sender_state_id",
        "sender_city_id",
    )
    warehouse_id = wire.get("warehouse_id")
    if warehouse_id is not None and (
        isinstance(warehouse_id, bool) or not isinstance(warehouse_id, int) or warehouse_id < 1
    ):
        errors.append("warehouse_id must be a positive integer.")
    if warehouse_id is not None and any(field in wire for field in manual_fields):
        errors.append("warehouse_id cannot be combined with manual sender fields.")
    if warehouse_id is None:
        for field in ("sender_name", "sender_phone", "sender_address", "sender_state_id", "sender_city_id"):
            if field not in wire:
                errors.append(f"{field} is required without warehouse_id.")
    if errors:
        raise ValueError(" ".join(errors))
    return wire


def patch_write(fields: Mapping[str, Any | UnsetType]) -> dict[str, Any]:
    wire = {key: value for key, value in fields.items() if value is not UNSET}
    if not wire:
        raise ValueError("PATCH must contain at least one present field.")
    buyer_phone = wire.get("buyer_phone")
    if buyer_phone is not None and (
        not isinstance(buyer_phone, str) or len(buyer_phone) != 10 or not buyer_phone.isdecimal()
    ):
        raise ValueError("buyer_phone must contain exactly 10 digits when supplied.")
    if "packages" in wire and (not isinstance(wire["packages"], list) or not wire["packages"]):
        raise ValueError("packages cannot be empty when supplied.")
    return wire


def public_webhook_url(url: str) -> bool:
    parsed = urlparse(url)
    hostname = parsed.hostname
    if parsed.scheme != "https" or not hostname or parsed.username is not None or parsed.password is not None:
        return False
    lowered = hostname.lower()
    if lowered == "localhost" or lowered.endswith((".localhost", ".local", ".internal")):
        return False
    try:
        return ipaddress.ip_address(hostname).is_global
    except ValueError:
        return "." in hostname


def warehouse_write(value: Mapping[str, Any]) -> dict[str, Any]:
    wire = dict(value)
    name = wire.get("name")
    contact_name = wire.get("contact_name")
    contact_phone = wire.get("contact_phone")
    address = wire.get("address")
    tax_number = wire.get("tax_number")
    if not isinstance(name, str) or not 2 <= len(name.strip()) <= 255:
        raise ValueError("Warehouse name must be 2-255 characters.")
    if (
        not isinstance(wire.get("is_main"), bool)
        or not isinstance(contact_name, str)
        or not contact_name.strip()
    ):
        raise ValueError("Warehouse is_main and contact_name are required.")
    if not isinstance(contact_phone, str) or len(contact_phone) != 10 or not contact_phone.isdecimal():
        raise ValueError("Warehouse contact_phone must contain 10 digits.")
    if not isinstance(address, str) or not 10 <= len(address.strip()) <= 512:
        raise ValueError("Warehouse address must be 10-512 characters.")
    for field in ("state_id", "city_id"):
        field_value = wire.get(field)
        if isinstance(field_value, bool) or not isinstance(field_value, int) or field_value < 1:
            raise ValueError(f"Warehouse {field} must be a positive integer.")
    if not isinstance(tax_number, str) or not 10 <= len(tax_number) <= 11 or not tax_number.isdecimal():
        raise ValueError("Warehouse tax_number must contain 10-11 digits.")
    return wire


class SyncExecutor(Protocol):
    def request_json(
        self,
        method: str,
        path: str,
        *,
        params: dict[str, int | str] | None = None,
        json_body: dict[str, Any] | None = None,
        form: dict[str, str] | None = None,
    ) -> Any: ...

    def request_void(
        self,
        method: str,
        path: str,
        *,
        json_body: dict[str, Any] | None = None,
        form: dict[str, str] | None = None,
    ) -> None: ...


def positive(value: int, label: str = "id") -> None:
    if value < 1:
        raise ValueError(f"{label} must be positive.")


def shipment_page(value: Any) -> ShipmentPage:
    payload = object_value(value)
    items = tuple(shipment(item) for item in envelope_list(payload))
    meta = object_value(payload.get("meta", {}), "pagination metadata")
    links = object_value(payload.get("links", {}), "pagination links")
    return ShipmentPage(
        items=items,
        current_page=integer(meta.get("current_page", 1), "current_page"),
        last_page=integer(meta.get("last_page", 1), "last_page"),
        per_page=integer(meta.get("per_page", len(items)), "per_page"),
        total=integer(meta.get("total", len(items)), "total"),
        next=nullable_text(links.get("next")),
        previous=nullable_text(links.get("prev")),
    )


def price_comparison(value: Any) -> PriceComparison:
    payload = object_value(value)
    raw_offers = payload.get("shipping_provider_with_price")
    if not isinstance(raw_offers, list):
        raise UnexpectedResponseError("Price comparison response has no offers.")
    offers: list[PriceOffer] = []
    for value_item in raw_offers:
        item = object_value(value_item, "price offer")
        provider_id = integer(item.get("id"), "price_offer.id")
        raw_price = nullable_text(item.get("price"))
        amount = None
        if raw_price:
            match = re.match(r"^\s*(\d+(?:[.,]\d+)?)", raw_price)
            if match:
                try:
                    amount = Decimal(match.group(1).replace(",", "."))
                except InvalidOperation as exception:
                    raise UnexpectedResponseError("Price offer contains an invalid decimal.") from exception
        offers.append(
            PriceOffer(
                provider_id=provider_id,
                provider_name=text(item.get("name"), "price_offer.name"),
                provider_slug=nullable_text(item.get("slug")),
                raw_price=raw_price,
                amount=amount,
                available=amount is not None or provider_id == -1,
            )
        )
    raw_shipment = payload.get("shipment")
    return PriceComparison(tuple(offers), shipment(raw_shipment) if isinstance(raw_shipment, dict) else None)


def webhook(value: Any) -> Webhook:
    data = envelope_object(value)
    known = {"id", "name", "url", "event_type", "is_active"}
    return Webhook(
        id=integer(data.get("id"), "webhook.id"),
        name=nullable_text(data.get("name")),
        url=text(data.get("url"), "webhook.url"),
        event_type=text(data.get("event_type"), "webhook.event_type"),
        is_active=bool(data.get("is_active", True)),
        extra={key: item for key, item in data.items() if key not in known},
    )


class ShipmentsResource:
    def __init__(self, executor: SyncExecutor) -> None:
        self._executor = executor

    def list(self, page: int = 1) -> ShipmentPage:
        positive(page, "page")
        return shipment_page(self._executor.request_json("GET", "/shipments", params={"page": page}))

    def get(self, shipment_id: int) -> Shipment:
        positive(shipment_id, "shipment_id")
        return shipment(envelope_object(self._executor.request_json("GET", f"/shipments/{shipment_id}")))

    def create(self, value: Mapping[str, Any]) -> Shipment:
        wire = self._validate_write(value)
        return shipment(
            envelope_object(self._executor.request_json("POST", "/shipments", json_body={"shipment": wire}))
        )

    def update(self, shipment_id: int, value: Mapping[str, Any]) -> Shipment:
        positive(shipment_id, "shipment_id")
        wire = self._validate_write(value)
        result = self._executor.request_json("PUT", f"/shipments/{shipment_id}", json_body={"shipment": wire})
        return shipment(envelope_object(result))

    def patch(self, shipment_id: int, **fields: Any | UnsetType) -> Shipment:
        positive(shipment_id, "shipment_id")
        wire = patch_write(fields)
        result = self._executor.request_json(
            "PATCH", f"/shipments/{shipment_id}", json_body={"shipment": wire}
        )
        return shipment(envelope_object(result))

    def delete(self, shipment_id: int) -> None:
        positive(shipment_id, "shipment_id")
        self._executor.request_void("DELETE", f"/shipments/{shipment_id}")

    def cancel(self, shipment_id: int) -> None:
        positive(shipment_id, "shipment_id")
        self._executor.request_void("POST", "/shipments/cancel", json_body={"shipment_id": shipment_id})

    def iter_all(self) -> Generator[Shipment]:
        page_number = 1
        visited: set[int] = set()
        while page_number not in visited:
            visited.add(page_number)
            page = self.list(page_number)
            yield from page.items
            if page.current_page >= page.last_page or page.next is None:
                return
            page_number = page.current_page + 1

    @staticmethod
    def _validate_write(value: Mapping[str, Any]) -> dict[str, Any]:
        return shipment_write(value)


class PricingResource:
    def __init__(self, executor: SyncExecutor) -> None:
        self._executor = executor

    def compare(self, shipment_id: int) -> PriceComparison:
        positive(shipment_id, "shipment_id")
        return price_comparison(
            self._executor.request_json("GET", f"/shipment-price-comparison/{shipment_id}")
        )

    def confirm(self, shipment_id: int, shipping_provider_id: int) -> None:
        positive(shipment_id, "shipment_id")
        if shipping_provider_id == 0 or shipping_provider_id < -1:
            raise ValueError("shipping_provider_id must be -1 or positive.")
        self._executor.request_void(
            "POST",
            "/confirm-shipping-price",
            form={"shipment_id": str(shipment_id), "shipping_provider_id": str(shipping_provider_id)},
        )


class AccountResource:
    def __init__(self, executor: SyncExecutor) -> None:
        self._executor = executor

    def credit(self) -> Credit:
        data = envelope_object(self._executor.request_json("GET", "/user/credit"))
        amount = decimal(data.get("credit"), "credit")
        if amount is None:
            raise UnexpectedResponseError("Credit response is null.")
        return Credit(amount)


class LocationsResource:
    def __init__(self, executor: SyncExecutor) -> None:
        self._executor = executor

    def states(self, country_id: int | None = None) -> tuple[State, ...]:
        if country_id is not None:
            positive(country_id, "country_id")
        path = "/states" if country_id is None else f"/states/{country_id}"
        return tuple(
            State(integer(item.get("id"), "state.id"), text(item.get("name"), "state.name"))
            for item in envelope_list(self._executor.request_json("GET", path))
        )

    def cities(self, state_id: int) -> tuple[City, ...]:
        positive(state_id, "state_id")
        return tuple(
            City(
                integer(item.get("id"), "city.id"),
                integer(item.get("state_id", state_id), "city.state_id"),
                text(item.get("name"), "city.name"),
            )
            for item in envelope_list(self._executor.request_json("GET", f"/cities/{state_id}"))
        )


class BarcodesResource:
    def __init__(self, executor: SyncExecutor) -> None:
        self._executor = executor

    def download(self, shipment_id: int) -> BarcodeDocument:
        positive(shipment_id, "shipment_id")
        data = envelope_object(
            self._executor.request_json("GET", f"/shipments/{shipment_id}/barcode", params={"format": "pdf"})
        )
        encoded = text(data.get("data"), "barcode.data")
        try:
            content = base64.b64decode(encoded, validate=True)
        except ValueError as exception:
            raise UnexpectedResponseError("Barcode response is not valid Base64.") from exception
        if len(content) > 25 * 1024 * 1024 or not content.startswith(b"%PDF"):
            raise UnexpectedResponseError("Kargonomi barcode is invalid PDF content.")
        return BarcodeDocument(content, encoded)


class WebhooksResource:
    def __init__(self, executor: SyncExecutor) -> None:
        self._executor = executor

    def list(self) -> tuple[Webhook, ...]:
        return tuple(webhook(item) for item in envelope_list(self._executor.request_json("GET", "/webhooks")))

    def get(self, webhook_id: int) -> Webhook:
        positive(webhook_id, "webhook_id")
        return webhook(self._executor.request_json("GET", f"/webhooks/{webhook_id}"))

    def create(
        self, *, url: str, event_type: str, name: str | None = None, is_active: bool = True
    ) -> Webhook:
        wire = self._wire(url, event_type, name, is_active)
        return webhook(self._executor.request_json("POST", "/webhooks", json_body=wire))

    def update(
        self, webhook_id: int, *, url: str, event_type: str, name: str | None = None, is_active: bool = True
    ) -> None:
        positive(webhook_id, "webhook_id")
        self._executor.request_void(
            "PUT", f"/webhooks/{webhook_id}", json_body=self._wire(url, event_type, name, is_active)
        )

    def delete(self, webhook_id: int) -> None:
        positive(webhook_id, "webhook_id")
        self._executor.request_void("DELETE", f"/webhooks/{webhook_id}")

    @staticmethod
    def _wire(url: str, event_type: str, name: str | None, is_active: bool) -> dict[str, Any]:
        if not public_webhook_url(url):
            raise ValueError("Webhook URL must target a public HTTPS host without user information.")
        if not event_type.strip():
            raise ValueError("event_type is required.")
        result: dict[str, Any] = {"url": url, "event_type": event_type, "is_active": is_active}
        if name is not None:
            result["name"] = name
        return result


class WarehousesResource:
    def __init__(self, executor: SyncExecutor) -> None:
        self._executor = executor

    def create(self, value: Mapping[str, Any]) -> Warehouse:
        wire = warehouse_write(value)
        data = envelope_object(
            self._executor.request_json("POST", "/warehouses", json_body={"warehouse": wire})
        )
        return Warehouse(
            integer(data.get("id"), "warehouse.id"),
            text(data.get("name"), "warehouse.name"),
            {key: item for key, item in data.items() if key not in {"id", "name"}},
        )
