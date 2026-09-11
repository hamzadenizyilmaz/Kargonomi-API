import { decodeCancellation, decodeCollection, decodeCredit, decodeLocation, decodePriceComparison, decodeShipment, decodeShipmentPage, decodeWebhook, record } from './decoders.ts';
import { UnexpectedResponseError } from './errors.ts';
import type { Transport } from './transport.ts';
import { assertPositiveId, type AccountCredit, type BarcodeDocument, type BarcodeOptions, type CancellationRequestResult, type Location, type PriceComparison, type Shipment, type ShipmentPage, type ShipmentPatchInput, type ShipmentWriteInput, type ShippingProviderSelection, type Warehouse, type WarehouseCreateInput, type Webhook, type WebhookWriteInput } from './types.ts';
import { validatePatch, validateShipmentWrite, validateWarehouse, validateWebhook } from './validation.ts';

export class ShipmentsResource {
  constructor(private readonly transport: Transport) {}
  list(page = 1, signal?: AbortSignal): Promise<ShipmentPage> { assertPositiveId(page, 'page'); return this.transport.json('GET', `shipments?page=${String(page)}`, decodeShipmentPage, undefined, signal); }
  get(id: number, signal?: AbortSignal): Promise<Shipment> { assertPositiveId(id); return this.transport.json('GET', `shipments/${String(id)}`, decodeShipment, undefined, signal); }
  create(input: ShipmentWriteInput, signal?: AbortSignal): Promise<Shipment> { validateShipmentWrite(input); return this.transport.json('POST', 'shipments', decodeShipment, { shipment: shipmentWire(input) }, signal); }
  update(id: number, input: ShipmentWriteInput, signal?: AbortSignal): Promise<Shipment> { assertPositiveId(id); validateShipmentWrite(input); return this.transport.json('PUT', `shipments/${String(id)}`, decodeShipment, { shipment: shipmentWire(input) }, signal); }
  patch(id: number, input: ShipmentPatchInput, signal?: AbortSignal): Promise<Shipment> { assertPositiveId(id); validatePatch(input); return this.transport.json('PATCH', `shipments/${String(id)}`, decodeShipment, { shipment: patchWire(input) }, signal); }
  delete(id: number, signal?: AbortSignal): Promise<void> { assertPositiveId(id); return this.transport.empty('DELETE', `shipments/${String(id)}`, signal); }
  cancel(id: number, signal?: AbortSignal): Promise<CancellationRequestResult> { assertPositiveId(id); return this.transport.json('POST', 'shipments/cancel', decodeCancellation, { shipment_id: id }, signal); }
  async *iterate(signal?: AbortSignal): AsyncGenerator<Shipment> {
    const visited = new Set<number>(); let pageNumber = 1;
    while (!visited.has(pageNumber)) { visited.add(pageNumber); const page = await this.list(pageNumber, signal); yield* page.items; if (page.currentPage >= page.lastPage || page.next === null) return; pageNumber = page.currentPage + 1; }
  }
}

export class PricingResource {
  constructor(private readonly transport: Transport) {}
  compare(id: number, signal?: AbortSignal): Promise<PriceComparison> { assertPositiveId(id); return this.transport.json('GET', `shipment-price-comparison/${String(id)}`, decodePriceComparison, undefined, signal); }
  confirm(id: number, provider: ShippingProviderSelection, signal?: AbortSignal): Promise<PriceComparison> {
    assertPositiveId(id); if (provider.id < -1 || provider.id === 0) throw new RangeError('shipping provider must be -1 or positive.');
    const form = new FormData(); form.set('shipment_id', String(id)); form.set('shipping_provider_id', String(provider.id));
    return this.transport.json('POST', 'confirm-shipping-price', decodePriceComparison, form, signal);
  }
}

export class AccountResource { constructor(private readonly transport: Transport) {} credit(signal?: AbortSignal): Promise<AccountCredit> { return this.transport.json('GET', 'user/credit', decodeCredit, undefined, signal); } }

export class WarehousesResource {
  constructor(private readonly transport: Transport) {}
  create(input: WarehouseCreateInput, signal?: AbortSignal): Promise<Warehouse> { validateWarehouse(input); return this.transport.json('POST', 'warehouses', (value) => { const raw = record(value, 'warehouse'); return { ...raw, id: Number(raw.id), name: String(raw.name) }; }, { warehouse: warehouseWire(input) }, signal); }
}

export class LocationsResource {
  constructor(private readonly transport: Transport) {}
  states(countryId?: number, signal?: AbortSignal): Promise<readonly Location[]> { if (countryId !== undefined) assertPositiveId(countryId, 'countryId'); return this.transport.json('GET', countryId === undefined ? 'states' : `states/${String(countryId)}`, (value) => decodeCollection(value, decodeLocation), undefined, signal); }
  cities(stateId: number, signal?: AbortSignal): Promise<readonly Location[]> { assertPositiveId(stateId, 'stateId'); return this.transport.json('GET', `cities/${String(stateId)}`, (value) => decodeCollection(value, decodeLocation), undefined, signal); }
}

export class BarcodesResource {
  constructor(private readonly transport: Transport) {}
  async pdf(id: number, options: BarcodeOptions = {}, signal?: AbortSignal): Promise<BarcodeDocument> {
    assertPositiveId(id); const query = new URLSearchParams({ format: 'pdf' });
    for (const [name, value] of Object.entries(options)) if (value !== undefined) query.set(name, String(value));
    const rawBase64 = await this.transport.json('GET', `shipments/${String(id)}/barcode?${query.toString()}`, (value) => String(record(value, 'barcode').data), undefined, signal);
    if (!/^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$/u.test(rawBase64)) throw new UnexpectedResponseError('Kargonomi barcode is invalid PDF content.');
    const bytes = Buffer.from(rawBase64, 'base64');
    if (bytes.toString('base64') !== rawBase64 || bytes.length > 25 * 1024 * 1024 || bytes.subarray(0, 4).toString('ascii') !== '%PDF') throw new UnexpectedResponseError('Kargonomi barcode is invalid PDF content.');
    return { bytes, rawBase64 };
  }
}

export class WebhooksResource {
  constructor(private readonly transport: Transport) {}
  list(signal?: AbortSignal): Promise<readonly Webhook[]> { return this.transport.json('GET', 'webhooks', (value) => decodeCollection(value, decodeWebhook), undefined, signal); }
  get(id: number, signal?: AbortSignal): Promise<Webhook> { assertPositiveId(id); return this.transport.json('GET', `webhooks/${String(id)}`, decodeWebhook, undefined, signal); }
  create(input: WebhookWriteInput, signal?: AbortSignal): Promise<Webhook> { validateWebhook(input); return this.transport.json('POST', 'webhooks', decodeWebhook, webhookWire(input), signal); }
  update(id: number, input: WebhookWriteInput, signal?: AbortSignal): Promise<Webhook> { assertPositiveId(id); validateWebhook(input); return this.transport.json('PUT', `webhooks/${String(id)}`, decodeWebhook, webhookWire(input), signal); }
  delete(id: number, signal?: AbortSignal): Promise<void> { assertPositiveId(id); return this.transport.empty('DELETE', `webhooks/${String(id)}`, signal); }
}

function shipmentWire(input: ShipmentWriteInput): Record<string, unknown> {
  const wire: Record<string, unknown> = {};
  for (const [key, value] of Object.entries(input)) if (value !== undefined) wire[snake(key)] = key === 'packages' ? input.packages.map(packageWire) : value;
  return wire;
}
function patchWire(input: ShipmentPatchInput): Record<string, unknown> { return Object.fromEntries(Object.entries(input).filter(([, value]) => value !== undefined).map(([key, value]) => [snake(key), key === 'packages' && Array.isArray(value) ? value.map(packageWire) : value])); }
function packageWire(value: { readonly desi: string; readonly content?: string | null; readonly barcode?: string | null }): Record<string, unknown> { return Object.fromEntries(Object.entries(value)); }
function warehouseWire(input: WarehouseCreateInput): Record<string, unknown> { return Object.fromEntries(Object.entries(input).map(([key, value]) => [snake(key), value])); }
function webhookWire(input: WebhookWriteInput): Record<string, unknown> { return { name: input.name, url: input.url, event_type: input.eventType, is_active: input.isActive ?? true }; }
function snake(value: string): string { return value.replace(/[A-Z]/gu, (letter) => `_${letter.toLocaleLowerCase('en-US')}`); }
