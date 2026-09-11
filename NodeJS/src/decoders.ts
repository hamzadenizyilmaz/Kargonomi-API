import type { AccountCredit, CancellationRequestResult, Location, PriceComparison, PriceOffer, Shipment, ShipmentPackage, ShipmentPage, ShipmentParty, ShipmentPricing, ShipmentWarehouse, Webhook } from './types.ts';

export function record(value: unknown, label: string): Record<string, unknown> {
  if (typeof value !== 'object' || value === null || Array.isArray(value)) throw new TypeError(`${label} must be an object.`);
  return value as Record<string, unknown>;
}

function text(value: unknown, label: string): string {
  if (typeof value !== 'string') throw new TypeError(`${label} must be a string.`);
  return value;
}

function nullableText(value: unknown): string | null { return value == null ? null : text(value, 'value'); }
function nullableDecimalText(value: unknown): string | null {
  if (value == null) return null;
  if (typeof value !== 'string' && typeof value !== 'number') throw new TypeError('decimal value must be a string or number.');
  return String(value);
}
function number(value: unknown, label: string): number {
  const parsed = typeof value === 'string' && /^-?\d+$/.test(value) ? Number(value) : value;
  if (typeof parsed !== 'number' || !Number.isSafeInteger(parsed)) throw new TypeError(`${label} must be a safe integer.`);
  return parsed;
}

function decodeParty(value: unknown, prefix: 'sender' | 'buyer'): ShipmentParty | null {
  if (value == null) return null;
  const raw = record(value, `${prefix} party`);
  return {
    name: nullableText(raw[`${prefix}_name`]), email: nullableText(raw[`${prefix}_email`]),
    phone: nullableText(raw[`${prefix}_phone`]), phone2: nullableText(raw[`${prefix}_phone2`]),
    taxNumber: nullableText(raw[`${prefix}_tax_number`]), taxPlace: nullableText(raw[`${prefix}_tax_place`]),
    address: nullableText(raw[`${prefix}_address`]), state: nullableText(raw[`${prefix}_state`]), city: nullableText(raw[`${prefix}_city`])
  };
}

function decodePricing(value: unknown): ShipmentPricing | null {
  if (value == null) return null;
  const raw = record(value, 'shipment pricing');
  return { packageCount: number(raw.package_count, 'pricing.package_count'), estimatedPrice: nullableDecimalText(raw.estimated_price), realPrice: nullableDecimalText(raw.real_price), extraShippingPrice: nullableDecimalText(raw.extra_shipping_price), priceDifference: nullableDecimalText(raw.price_diff) };
}

function decodePackage(value: unknown): ShipmentPackage {
  const raw = record(value, 'shipment package');
  const known = new Set(['desi', 'barcode', 'content', 'real_desi']);
  return { desi: text(raw.desi, 'package.desi'), barcode: nullableText(raw.barcode), content: nullableText(raw.content), realDesi: nullableText(raw.real_desi), additionalData: Object.fromEntries(Object.entries(raw).filter(([key]) => !known.has(key))) };
}

function decodeWarehouse(value: unknown): ShipmentWarehouse | null {
  if (value == null) return null;
  const raw = record(value, 'shipment warehouse');
  const known = new Set(['id', 'name', 'is_main', 'contact_name', 'contact_phone', 'state', 'city', 'address', 'phone', 'tax_number', 'tax_place']);
  if (typeof raw.is_main !== 'boolean' && raw.is_main !== 0 && raw.is_main !== 1) throw new TypeError('warehouse.is_main must be Boolean or 0/1.');
  return { id: number(raw.id, 'warehouse.id'), name: text(raw.name, 'warehouse.name'), isMain: Boolean(raw.is_main), contactName: nullableText(raw.contact_name), contactPhone: nullableText(raw.contact_phone), state: nullableText(raw.state), city: nullableText(raw.city), address: nullableText(raw.address), phone: nullableText(raw.phone), taxNumber: nullableText(raw.tax_number), taxPlace: nullableText(raw.tax_place), additionalData: Object.fromEntries(Object.entries(raw).filter(([key]) => !known.has(key))) };
}

export function decodeShipment(value: unknown): Shipment {
  const raw = record(value, 'shipment');
  const known = new Set([
    'id', 'type', 'status', 'status_label', 'package_count', 'extra_shipping_price', 'buyer_name', 'created_at', 'updated_at',
    'shipping_webservice_order_id', 'shipping_webservice_barcode', 'shipping_webservice_tracking_code', 'shipping_provider_name',
    'shipping_provider_slug', 'barcode_of_order_id', 'shipment_packages', 'sender', 'buyer', 'pricing', 'warehouse',
    'shipping_webservice_created_at', 'ecommerce_provider_order_no', 'ecommerce_provider', 'estimated_price', 'real_price',
    'delivery_date_to_shipment_office', 'shipping_provider_customer_delivery_date'
  ]);
  const additionalData = Object.fromEntries(Object.entries(raw).filter(([key]) => !known.has(key)));
  const packages = raw.shipment_packages;
  if (!Array.isArray(packages)) throw new TypeError('shipment.shipment_packages must be an array.');
  return {
    id: number(raw.id, 'shipment.id'), type: text(raw.type, 'shipment.type'), status: text(raw.status, 'shipment.status'),
    statusLabel: text(raw.status_label, 'shipment.status_label'), packageCount: number(raw.package_count, 'shipment.package_count'),
    estimatedPrice: nullableDecimalText(raw.estimated_price), realPrice: nullableDecimalText(raw.real_price), extraShippingPrice: nullableDecimalText(raw.extra_shipping_price), buyerName: nullableText(raw.buyer_name),
    createdAt: text(raw.created_at, 'shipment.created_at'), updatedAt: text(raw.updated_at, 'shipment.updated_at'),
    shippingWebserviceOrderId: nullableText(raw.shipping_webservice_order_id), shippingWebserviceBarcode: nullableText(raw.shipping_webservice_barcode),
    shippingWebserviceTrackingCode: nullableText(raw.shipping_webservice_tracking_code), shippingProviderName: nullableText(raw.shipping_provider_name),
    shippingProviderSlug: nullableText(raw.shipping_provider_slug), barcodeOfOrderId: nullableText(raw.barcode_of_order_id),
    shippingWebserviceCreatedAt: nullableText(raw.shipping_webservice_created_at), ecommerceProviderOrderNo: nullableText(raw.ecommerce_provider_order_no), ecommerceProvider: nullableText(raw.ecommerce_provider),
    deliveryDateToShipmentOffice: nullableText(raw.delivery_date_to_shipment_office), shippingProviderCustomerDeliveryDate: nullableText(raw.shipping_provider_customer_delivery_date),
    shipmentPackages: packages.map(decodePackage), sender: decodeParty(raw.sender, 'sender'), buyer: decodeParty(raw.buyer, 'buyer'),
    pricing: decodePricing(raw.pricing), warehouse: decodeWarehouse(raw.warehouse), additionalData
  };
}

export function decodeShipmentPage(value: unknown): ShipmentPage {
  const root = record(value, 'shipment page');
  const data = root.data;
  const links = record(root.links, 'shipment page links');
  const meta = record(root.meta, 'shipment page meta');
  if (!Array.isArray(data)) throw new TypeError('shipment page data must be an array.');
  return { items: data.map(decodeShipment), currentPage: number(meta.current_page, 'current_page'), lastPage: number(meta.last_page, 'last_page'), perPage: number(meta.per_page, 'per_page'), total: number(meta.total, 'total'), next: nullableText(links.next), previous: nullableText(links.prev) };
}

function decodeOffer(value: unknown): PriceOffer {
  const raw = record(value, 'price offer');
  const rawPrice = nullableText(raw.price);
  const leading = rawPrice?.match(/^\d+(?:\.\d+)?/)?.[0] ?? null;
  return { id: number(raw.id, 'price offer id'), name: text(raw.name, 'price offer name'), slug: text(raw.slug, 'price offer slug'), rawPrice, parsedAmount: leading, available: rawPrice?.toLocaleLowerCase('tr-TR') !== 'hizmet dışı bölge' };
}

export function decodePriceComparison(value: unknown): PriceComparison {
  const raw = record(value, 'price comparison');
  if (!Array.isArray(raw.shipping_provider_with_price)) throw new TypeError('shipping_provider_with_price must be an array.');
  return { offers: raw.shipping_provider_with_price.map(decodeOffer), shipment: decodeShipment(raw.shipment) };
}

export function decodeCredit(value: unknown): AccountCredit {
  const amount = record(record(value, 'credit response').data, 'credit data').credit;
  if (typeof amount !== 'string' && typeof amount !== 'number') throw new TypeError('credit must be a number or decimal string.');
  return { amount: String(amount) };
}

export function decodeCancellation(value: unknown): CancellationRequestResult {
  const raw = record(value, 'cancellation result');
  return { message: text(raw.message, 'message'), shipmentId: number(raw.shipment_id, 'shipment_id') };
}

export function decodeCollection<T>(value: unknown, decode: (item: unknown) => T): readonly T[] {
  const data = record(value, 'collection response').data;
  if (!Array.isArray(data)) throw new TypeError('collection data must be an array.');
  return data.map(decode);
}

export function decodeLocation(value: unknown): Location {
  const raw = record(value, 'location');
  const location: Location = { ...raw, id: number(raw.id, 'location id'), name: text(raw.name, 'location name') };
  return raw.state_id === undefined ? location : { ...location, stateId: number(raw.state_id, 'state_id') };
}

export function decodeWebhook(value: unknown): Webhook {
  const raw = record(value, 'webhook');
  const known = new Set(['id', 'name', 'url', 'event_type', 'is_active']);
  return { id: number(raw.id, 'webhook id'), name: text(raw.name, 'webhook name'), url: text(raw.url, 'webhook url'), eventType: text(raw.event_type, 'webhook event type'), isActive: Boolean(raw.is_active), additionalData: Object.fromEntries(Object.entries(raw).filter(([key]) => !known.has(key))) };
}
