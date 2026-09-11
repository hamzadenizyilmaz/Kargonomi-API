from __future__ import annotations

from dataclasses import dataclass, field
from datetime import datetime
from decimal import Decimal, InvalidOperation
from typing import Any

from .errors import UnexpectedResponseError

KNOWN_STATUSES = frozenset(
    {
        "draft",
        "ready",
        "webservice_order_failed",
        "webservice_order_creating",
        "webservice_order_created",
        "webservice_checking_shipment",
        "webservice_shipment_started",
        "webservice_shipment_delivered",
        "webservice_shipment_not_delivered",
        "webservice_shipment_returning",
        "webservice_shipment_missing",
        "cancelled",
        "request_for_cancellation",
    }
)


@dataclass(frozen=True, slots=True)
class ShipmentStatus:
    value: str

    @property
    def is_known(self) -> bool:
        return self.value in KNOWN_STATUSES


@dataclass(frozen=True, slots=True)
class ShipmentParty:
    name: str | None
    email: str | None
    phone: str | None
    phone2: str | None
    tax_number: str | None
    tax_place: str | None
    address: str | None
    state: str | None
    city: str | None


@dataclass(frozen=True, slots=True)
class ShipmentPricing:
    package_count: int
    estimated_price: Decimal | None
    real_price: Decimal | None
    extra_shipping_price: Decimal | None
    price_difference: Decimal | None


@dataclass(frozen=True, slots=True)
class ShipmentPackage:
    desi: str
    barcode: str | None
    content: str | None
    real_desi: str | None
    extra: dict[str, Any] = field(default_factory=dict)


@dataclass(frozen=True, slots=True)
class ShipmentWarehouse:
    id: int
    name: str
    is_main: bool
    contact_name: str | None
    contact_phone: str | None
    state: str | None
    city: str | None
    address: str | None
    phone: str | None
    tax_number: str | None
    tax_place: str | None
    extra: dict[str, Any] = field(default_factory=dict)


@dataclass(frozen=True, slots=True)
class Shipment:
    id: int
    status: ShipmentStatus
    status_label: str | None
    type: str | None
    package_count: int | None
    estimated_price: Decimal | None
    real_price: Decimal | None
    extra_shipping_price: Decimal | None
    buyer_name: str | None
    shipping_webservice_order_id: str | None
    shipping_webservice_barcode: str | None
    shipping_webservice_tracking_code: str | None
    shipping_provider_name: str | None
    shipping_provider_slug: str | None
    barcode_of_order_id: str | None
    shipping_webservice_created_at: datetime | None
    ecommerce_provider_order_no: str | None
    ecommerce_provider: str | None
    delivery_date_to_shipment_office: datetime | None
    shipping_provider_customer_delivery_date: datetime | None
    created_at: datetime | None
    updated_at: datetime | None
    sender: ShipmentParty | None
    buyer: ShipmentParty | None
    pricing: ShipmentPricing | None
    warehouse: ShipmentWarehouse | None
    packages: tuple[ShipmentPackage, ...]
    extra: dict[str, Any] = field(default_factory=dict)


@dataclass(frozen=True, slots=True)
class ShipmentPage:
    items: tuple[Shipment, ...]
    current_page: int
    last_page: int
    per_page: int
    total: int
    next: str | None
    previous: str | None


@dataclass(frozen=True, slots=True)
class PriceOffer:
    provider_id: int
    provider_name: str
    provider_slug: str | None
    raw_price: str | None
    amount: Decimal | None
    available: bool


@dataclass(frozen=True, slots=True)
class PriceComparison:
    offers: tuple[PriceOffer, ...]
    shipment: Shipment | None


@dataclass(frozen=True, slots=True)
class Credit:
    amount: Decimal


@dataclass(frozen=True, slots=True)
class State:
    id: int
    name: str


@dataclass(frozen=True, slots=True)
class City:
    id: int
    state_id: int
    name: str


@dataclass(frozen=True, slots=True)
class Webhook:
    id: int
    name: str | None
    url: str
    event_type: str
    is_active: bool
    extra: dict[str, Any]


@dataclass(frozen=True, slots=True)
class Warehouse:
    id: int
    name: str
    extra: dict[str, Any]


@dataclass(frozen=True, slots=True)
class BarcodeDocument:
    content: bytes
    raw_base64: str


def object_value(value: Any, label: str = "response") -> dict[str, Any]:
    if not isinstance(value, dict):
        raise UnexpectedResponseError(f"Expected {label} to be an object.")
    return {str(key): item for key, item in value.items()}


def envelope_object(value: Any) -> dict[str, Any]:
    payload = object_value(value)
    return object_value(payload.get("data", payload))


def envelope_list(value: Any) -> list[dict[str, Any]]:
    payload = object_value(value)
    items = payload.get("data", payload)
    if not isinstance(items, list):
        raise UnexpectedResponseError("Expected response data to be a list.")
    return [object_value(item, "collection item") for item in items]


def integer(value: Any, label: str) -> int:
    if isinstance(value, bool) or not isinstance(value, (int, str)):
        raise UnexpectedResponseError(f"Expected {label} to be an integer.")
    try:
        return int(value)
    except ValueError as exception:
        raise UnexpectedResponseError(f"Expected {label} to be an integer.") from exception


def text(value: Any, label: str) -> str:
    if not isinstance(value, str):
        raise UnexpectedResponseError(f"Expected {label} to be a string.")
    return value


def nullable_text(value: Any) -> str | None:
    return value if isinstance(value, str) else None


def decimal(value: Any, label: str) -> Decimal | None:
    if value is None:
        return None
    if isinstance(value, bool) or not isinstance(value, (str, int, float)):
        raise UnexpectedResponseError(f"Expected {label} to be decimal-compatible.")
    try:
        return Decimal(str(value))
    except InvalidOperation as exception:
        raise UnexpectedResponseError(f"Expected {label} to be decimal-compatible.") from exception


def date(value: Any) -> datetime | None:
    if not isinstance(value, str) or not value:
        return None
    try:
        parsed = datetime.fromisoformat(value.replace("Z", "+00:00"))
    except ValueError as exception:
        raise UnexpectedResponseError("Provider returned an invalid ISO 8601 timestamp.") from exception
    if parsed.tzinfo is None:
        raise UnexpectedResponseError("Provider timestamp must include a timezone offset.")
    return parsed


def shipment(value: Any) -> Shipment:
    data = object_value(value, "shipment")
    known = {
        "id",
        "status",
        "status_label",
        "type",
        "package_count",
        "estimated_price",
        "real_price",
        "extra_shipping_price",
        "buyer_name",
        "created_at",
        "updated_at",
        "shipment_packages",
        "shipping_webservice_order_id",
        "shipping_webservice_barcode",
        "shipping_webservice_tracking_code",
        "shipping_provider_name",
        "shipping_provider_slug",
        "barcode_of_order_id",
        "shipping_webservice_created_at",
        "ecommerce_provider_order_no",
        "ecommerce_provider",
        "delivery_date_to_shipment_office",
        "shipping_provider_customer_delivery_date",
        "sender",
        "buyer",
        "pricing",
        "warehouse",
    }
    raw_packages = data.get("shipment_packages", [])
    packages = (
        tuple(shipment_package(item) for item in raw_packages) if isinstance(raw_packages, list) else ()
    )
    return Shipment(
        id=integer(data.get("id"), "shipment.id"),
        status=ShipmentStatus(text(data.get("status"), "shipment.status")),
        status_label=nullable_text(data.get("status_label")),
        type=nullable_text(data.get("type")),
        package_count=integer(data["package_count"], "shipment.package_count")
        if data.get("package_count") is not None
        else None,
        estimated_price=decimal(data.get("estimated_price"), "shipment.estimated_price"),
        real_price=decimal(data.get("real_price"), "shipment.real_price"),
        extra_shipping_price=decimal(data.get("extra_shipping_price"), "shipment.extra_shipping_price"),
        buyer_name=nullable_text(data.get("buyer_name")),
        shipping_webservice_order_id=nullable_text(data.get("shipping_webservice_order_id")),
        shipping_webservice_barcode=nullable_text(data.get("shipping_webservice_barcode")),
        shipping_webservice_tracking_code=nullable_text(data.get("shipping_webservice_tracking_code")),
        shipping_provider_name=nullable_text(data.get("shipping_provider_name")),
        shipping_provider_slug=nullable_text(data.get("shipping_provider_slug")),
        barcode_of_order_id=nullable_text(data.get("barcode_of_order_id")),
        shipping_webservice_created_at=date(data.get("shipping_webservice_created_at")),
        ecommerce_provider_order_no=nullable_text(data.get("ecommerce_provider_order_no")),
        ecommerce_provider=nullable_text(data.get("ecommerce_provider")),
        delivery_date_to_shipment_office=date(data.get("delivery_date_to_shipment_office")),
        shipping_provider_customer_delivery_date=date(data.get("shipping_provider_customer_delivery_date")),
        created_at=date(data.get("created_at")),
        updated_at=date(data.get("updated_at")),
        sender=shipment_party(data.get("sender"), "sender"),
        buyer=shipment_party(data.get("buyer"), "buyer"),
        pricing=shipment_pricing(data.get("pricing")),
        warehouse=shipment_warehouse(data.get("warehouse")),
        packages=packages,
        extra={key: item for key, item in data.items() if key not in known},
    )


def shipment_party(value: Any, prefix: str) -> ShipmentParty | None:
    if value is None:
        return None
    data = object_value(value, f"shipment {prefix}")
    return ShipmentParty(
        nullable_text(data.get(f"{prefix}_name")),
        nullable_text(data.get(f"{prefix}_email")),
        nullable_text(data.get(f"{prefix}_phone")),
        nullable_text(data.get(f"{prefix}_phone2")),
        nullable_text(data.get(f"{prefix}_tax_number")),
        nullable_text(data.get(f"{prefix}_tax_place")),
        nullable_text(data.get(f"{prefix}_address")),
        nullable_text(data.get(f"{prefix}_state")),
        nullable_text(data.get(f"{prefix}_city")),
    )


def shipment_pricing(value: Any) -> ShipmentPricing | None:
    if value is None:
        return None
    data = object_value(value, "shipment pricing")
    return ShipmentPricing(
        integer(data.get("package_count"), "pricing.package_count"),
        decimal(data.get("estimated_price"), "pricing.estimated_price"),
        decimal(data.get("real_price"), "pricing.real_price"),
        decimal(data.get("extra_shipping_price"), "pricing.extra_shipping_price"),
        decimal(data.get("price_diff"), "pricing.price_diff"),
    )


def shipment_package(value: Any) -> ShipmentPackage:
    data = object_value(value, "shipment package")
    known = {"desi", "barcode", "content", "real_desi"}
    return ShipmentPackage(
        text(data.get("desi"), "package.desi"),
        nullable_text(data.get("barcode")),
        nullable_text(data.get("content")),
        nullable_text(data.get("real_desi")),
        {key: item for key, item in data.items() if key not in known},
    )


def shipment_warehouse(value: Any) -> ShipmentWarehouse | None:
    if value is None:
        return None
    data = object_value(value, "shipment warehouse")
    raw_main = data.get("is_main")
    if not isinstance(raw_main, (bool, int)) or raw_main not in (False, True, 0, 1):
        raise UnexpectedResponseError("Expected warehouse.is_main to be Boolean or 0/1.")
    known = {
        "id",
        "name",
        "is_main",
        "contact_name",
        "contact_phone",
        "state",
        "city",
        "address",
        "phone",
        "tax_number",
        "tax_place",
    }
    return ShipmentWarehouse(
        integer(data.get("id"), "warehouse.id"),
        text(data.get("name"), "warehouse.name"),
        bool(raw_main),
        nullable_text(data.get("contact_name")),
        nullable_text(data.get("contact_phone")),
        nullable_text(data.get("state")),
        nullable_text(data.get("city")),
        nullable_text(data.get("address")),
        nullable_text(data.get("phone")),
        nullable_text(data.get("tax_number")),
        nullable_text(data.get("tax_place")),
        {key: item for key, item in data.items() if key not in known},
    )
