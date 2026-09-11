export interface KargonomiClientOptions {
  readonly apiToken: string;
  readonly baseUrl?: string;
  readonly timeoutMs?: number;
  readonly fetch?: typeof globalThis.fetch;
  readonly maxRetries?: number;
  readonly retryDelayMs?: number;
}

export interface ShipmentPackageInput {
  readonly desi: string;
  readonly content?: string | null;
  readonly barcode?: string | null;
}

export interface ShipmentWriteInput {
  readonly senderName?: string;
  readonly senderEmail?: string | null;
  readonly senderTaxNumber?: string | null;
  readonly senderTaxPlace?: string | null;
  readonly senderPhone?: string;
  readonly senderAddress?: string;
  readonly senderStateId?: number;
  readonly senderCityId?: number;
  readonly warehouseId?: number;
  readonly buyerName: string;
  readonly buyerEmail?: string | null;
  readonly buyerTaxNumber?: string | null;
  readonly buyerTaxPlace?: string | null;
  readonly buyerPhone: string;
  readonly buyerAddress: string;
  readonly buyerStateId: number;
  readonly buyerCityId: number;
  readonly packages: readonly ShipmentPackageInput[];
}

export interface ShipmentPatchInput {
  readonly buyerName?: string | null;
  readonly buyerEmail?: string | null;
  readonly buyerPhone?: string | null;
  readonly buyerAddress?: string | null;
  readonly buyerStateId?: number | null;
  readonly buyerCityId?: number | null;
  readonly packages?: readonly ShipmentPackageInput[] | null;
}

export interface Shipment {
  readonly id: number;
  readonly type: string;
  readonly status: string;
  readonly statusLabel: string;
  readonly packageCount: number;
  readonly estimatedPrice: string | null;
  readonly realPrice: string | null;
  readonly extraShippingPrice: string | null;
  readonly buyerName: string | null;
  readonly createdAt: string;
  readonly updatedAt: string;
  readonly shippingWebserviceOrderId: string | null;
  readonly shippingWebserviceBarcode: string | null;
  readonly shippingWebserviceTrackingCode: string | null;
  readonly shippingProviderName: string | null;
  readonly shippingProviderSlug: string | null;
  readonly barcodeOfOrderId: string | null;
  readonly shippingWebserviceCreatedAt: string | null;
  readonly ecommerceProviderOrderNo: string | null;
  readonly ecommerceProvider: string | null;
  readonly deliveryDateToShipmentOffice: string | null;
  readonly shippingProviderCustomerDeliveryDate: string | null;
  readonly shipmentPackages: readonly ShipmentPackage[];
  readonly sender: ShipmentParty | null;
  readonly buyer: ShipmentParty | null;
  readonly pricing: ShipmentPricing | null;
  readonly warehouse: ShipmentWarehouse | null;
  readonly additionalData: Readonly<Record<string, unknown>>;
}

export interface ShipmentParty {
  readonly name: string | null;
  readonly email: string | null;
  readonly phone: string | null;
  readonly phone2: string | null;
  readonly taxNumber: string | null;
  readonly taxPlace: string | null;
  readonly address: string | null;
  readonly state: string | null;
  readonly city: string | null;
}

export interface ShipmentPricing {
  readonly packageCount: number;
  readonly estimatedPrice: string | null;
  readonly realPrice: string | null;
  readonly extraShippingPrice: string | null;
  readonly priceDifference: string | null;
}

export interface ShipmentPackage {
  readonly desi: string;
  readonly barcode: string | null;
  readonly content: string | null;
  readonly realDesi: string | null;
  readonly additionalData: Readonly<Record<string, unknown>>;
}

export interface ShipmentWarehouse {
  readonly id: number;
  readonly name: string;
  readonly isMain: boolean;
  readonly contactName: string | null;
  readonly contactPhone: string | null;
  readonly state: string | null;
  readonly city: string | null;
  readonly address: string | null;
  readonly phone: string | null;
  readonly taxNumber: string | null;
  readonly taxPlace: string | null;
  readonly additionalData: Readonly<Record<string, unknown>>;
}

export interface ShipmentPage {
  readonly items: readonly Shipment[];
  readonly currentPage: number;
  readonly lastPage: number;
  readonly perPage: number;
  readonly total: number;
  readonly next: string | null;
  readonly previous: string | null;
}

export interface PriceOffer {
  readonly id: number;
  readonly name: string;
  readonly slug: string;
  readonly rawPrice: string | null;
  readonly parsedAmount: string | null;
  readonly available: boolean;
}

export interface PriceComparison {
  readonly offers: readonly PriceOffer[];
  readonly shipment: Shipment;
}

export interface ShippingProviderSelection { readonly id: number }
export const automaticShippingProvider: ShippingProviderSelection = Object.freeze({ id: -1 });
export function shippingProvider(id: number): ShippingProviderSelection {
  assertPositiveId(id, 'shippingProviderId');
  return { id };
}

export interface WarehouseCreateInput {
  readonly name: string;
  readonly isMain: boolean;
  readonly contactName: string;
  readonly contactPhone: string;
  readonly address: string;
  readonly stateId: number;
  readonly cityId: number;
  readonly taxNumber: string;
}

export interface Warehouse extends Record<string, unknown> { readonly id: number; readonly name: string }
export interface Location extends Record<string, unknown> { readonly id: number; readonly name: string; readonly stateId?: number }

export interface BarcodeOptions {
  readonly packageContentVisibility?: boolean;
  readonly warningVisibility?: boolean;
  readonly integratedOrderNoVisibility?: boolean;
}

export interface BarcodeDocument { readonly bytes: Uint8Array; readonly rawBase64: string }

export interface Webhook {
  readonly id: number;
  readonly name: string;
  readonly url: string;
  readonly eventType: string;
  readonly isActive: boolean;
  readonly additionalData: Readonly<Record<string, unknown>>;
}

export interface WebhookWriteInput {
  readonly name: string;
  readonly url: string;
  readonly eventType: string;
  readonly isActive?: boolean;
}

export interface CancellationRequestResult { readonly message: string; readonly shipmentId: number }
export interface AccountCredit { readonly amount: string }

export function assertPositiveId(value: number, name = 'id'): void {
  if (!Number.isSafeInteger(value) || value <= 0) throw new RangeError(`${name} must be a positive safe integer.`);
}
