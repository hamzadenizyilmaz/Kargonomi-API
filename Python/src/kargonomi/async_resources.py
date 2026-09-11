from __future__ import annotations

import base64
from collections.abc import AsyncGenerator, Mapping
from typing import Any, Protocol

from .errors import UnexpectedResponseError
from .models import (
    BarcodeDocument,
    City,
    Credit,
    PriceComparison,
    Shipment,
    ShipmentPage,
    State,
    Warehouse,
    Webhook,
    decimal,
    envelope_list,
    envelope_object,
    integer,
    shipment,
    text,
)
from .resources import (
    UnsetType,
    WebhooksResource,
    patch_write,
    positive,
    price_comparison,
    shipment_page,
    shipment_write,
    warehouse_write,
    webhook,
)


class AsyncExecutor(Protocol):
    async def request_json(
        self,
        method: str,
        path: str,
        *,
        params: dict[str, int | str] | None = None,
        json_body: dict[str, Any] | None = None,
        form: dict[str, str] | None = None,
    ) -> Any: ...

    async def request_void(
        self,
        method: str,
        path: str,
        *,
        json_body: dict[str, Any] | None = None,
        form: dict[str, str] | None = None,
    ) -> None: ...


class AsyncShipmentsResource:
    def __init__(self, executor: AsyncExecutor) -> None:
        self._executor = executor

    async def list(self, page: int = 1) -> ShipmentPage:
        positive(page, "page")
        return shipment_page(await self._executor.request_json("GET", "/shipments", params={"page": page}))

    async def get(self, shipment_id: int) -> Shipment:
        positive(shipment_id, "shipment_id")
        return shipment(
            envelope_object(await self._executor.request_json("GET", f"/shipments/{shipment_id}"))
        )

    async def create(self, value: Mapping[str, Any]) -> Shipment:
        wire = self._write(value)
        result = await self._executor.request_json("POST", "/shipments", json_body={"shipment": wire})
        return shipment(envelope_object(result))

    async def update(self, shipment_id: int, value: Mapping[str, Any]) -> Shipment:
        positive(shipment_id, "shipment_id")
        result = await self._executor.request_json(
            "PUT", f"/shipments/{shipment_id}", json_body={"shipment": self._write(value)}
        )
        return shipment(envelope_object(result))

    async def patch(self, shipment_id: int, **fields: Any | UnsetType) -> Shipment:
        positive(shipment_id, "shipment_id")
        wire = patch_write(fields)
        result = await self._executor.request_json(
            "PATCH", f"/shipments/{shipment_id}", json_body={"shipment": wire}
        )
        return shipment(envelope_object(result))

    async def delete(self, shipment_id: int) -> None:
        positive(shipment_id, "shipment_id")
        await self._executor.request_void("DELETE", f"/shipments/{shipment_id}")

    async def cancel(self, shipment_id: int) -> None:
        positive(shipment_id, "shipment_id")
        await self._executor.request_void("POST", "/shipments/cancel", json_body={"shipment_id": shipment_id})

    async def iter_all(self) -> AsyncGenerator[Shipment]:
        page_number = 1
        visited: set[int] = set()
        while page_number not in visited:
            visited.add(page_number)
            page = await self.list(page_number)
            for item in page.items:
                yield item
            if page.current_page >= page.last_page or page.next is None:
                return
            page_number = page.current_page + 1

    @staticmethod
    def _write(value: Mapping[str, Any]) -> dict[str, Any]:
        return shipment_write(value)


class AsyncPricingResource:
    def __init__(self, executor: AsyncExecutor) -> None:
        self._executor = executor

    async def compare(self, shipment_id: int) -> PriceComparison:
        positive(shipment_id, "shipment_id")
        return price_comparison(
            await self._executor.request_json("GET", f"/shipment-price-comparison/{shipment_id}")
        )

    async def confirm(self, shipment_id: int, shipping_provider_id: int) -> None:
        positive(shipment_id, "shipment_id")
        if shipping_provider_id == 0 or shipping_provider_id < -1:
            raise ValueError("shipping_provider_id must be -1 or positive.")
        await self._executor.request_void(
            "POST",
            "/confirm-shipping-price",
            form={"shipment_id": str(shipment_id), "shipping_provider_id": str(shipping_provider_id)},
        )


class AsyncAccountResource:
    def __init__(self, executor: AsyncExecutor) -> None:
        self._executor = executor

    async def credit(self) -> Credit:
        data = envelope_object(await self._executor.request_json("GET", "/user/credit"))
        amount = decimal(data.get("credit"), "credit")
        if amount is None:
            raise UnexpectedResponseError("Credit response is null.")
        return Credit(amount)


class AsyncLocationsResource:
    def __init__(self, executor: AsyncExecutor) -> None:
        self._executor = executor

    async def states(self, country_id: int | None = None) -> tuple[State, ...]:
        if country_id is not None:
            positive(country_id, "country_id")
        path = "/states" if country_id is None else f"/states/{country_id}"
        items = envelope_list(await self._executor.request_json("GET", path))
        return tuple(
            State(integer(item.get("id"), "state.id"), text(item.get("name"), "state.name")) for item in items
        )

    async def cities(self, state_id: int) -> tuple[City, ...]:
        positive(state_id, "state_id")
        items = envelope_list(await self._executor.request_json("GET", f"/cities/{state_id}"))
        return tuple(
            City(
                integer(item.get("id"), "city.id"),
                integer(item.get("state_id", state_id), "city.state_id"),
                text(item.get("name"), "city.name"),
            )
            for item in items
        )


class AsyncBarcodesResource:
    def __init__(self, executor: AsyncExecutor) -> None:
        self._executor = executor

    async def download(self, shipment_id: int) -> BarcodeDocument:
        positive(shipment_id, "shipment_id")
        data = envelope_object(
            await self._executor.request_json(
                "GET", f"/shipments/{shipment_id}/barcode", params={"format": "pdf"}
            )
        )
        encoded = text(data.get("data"), "barcode.data")
        try:
            content = base64.b64decode(encoded, validate=True)
        except ValueError as exception:
            raise UnexpectedResponseError("Barcode response is not valid Base64.") from exception
        if len(content) > 25 * 1024 * 1024 or not content.startswith(b"%PDF"):
            raise UnexpectedResponseError("Kargonomi barcode is invalid PDF content.")
        return BarcodeDocument(content, encoded)


class AsyncWebhooksResource:
    def __init__(self, executor: AsyncExecutor) -> None:
        self._executor = executor

    async def list(self) -> tuple[Webhook, ...]:
        items = envelope_list(await self._executor.request_json("GET", "/webhooks"))
        return tuple(webhook(item) for item in items)

    async def get(self, webhook_id: int) -> Webhook:
        positive(webhook_id, "webhook_id")
        return webhook(await self._executor.request_json("GET", f"/webhooks/{webhook_id}"))

    async def create(
        self, *, url: str, event_type: str, name: str | None = None, is_active: bool = True
    ) -> Webhook:
        wire = WebhooksResource._wire(url, event_type, name, is_active)
        return webhook(await self._executor.request_json("POST", "/webhooks", json_body=wire))

    async def update(
        self, webhook_id: int, *, url: str, event_type: str, name: str | None = None, is_active: bool = True
    ) -> None:
        positive(webhook_id, "webhook_id")
        wire = WebhooksResource._wire(url, event_type, name, is_active)
        await self._executor.request_void("PUT", f"/webhooks/{webhook_id}", json_body=wire)

    async def delete(self, webhook_id: int) -> None:
        positive(webhook_id, "webhook_id")
        await self._executor.request_void("DELETE", f"/webhooks/{webhook_id}")


class AsyncWarehousesResource:
    def __init__(self, executor: AsyncExecutor) -> None:
        self._executor = executor

    async def create(self, value: Mapping[str, Any]) -> Warehouse:
        wire = warehouse_write(value)
        data = envelope_object(
            await self._executor.request_json("POST", "/warehouses", json_body={"warehouse": wire})
        )
        return Warehouse(
            integer(data.get("id"), "warehouse.id"),
            text(data.get("name"), "warehouse.name"),
            {key: item for key, item in data.items() if key not in {"id", "name"}},
        )
