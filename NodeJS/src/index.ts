import { AccountResource, BarcodesResource, LocationsResource, PricingResource, ShipmentsResource, WarehousesResource, WebhooksResource } from './resources.ts';
import { Transport } from './transport.ts';
import type { KargonomiClientOptions } from './types.ts';

export { AuthenticationError, KargonomiError, NetworkError, NotFoundError, RateLimitError, TimeoutError, UnexpectedResponseError, ValidationError } from './errors.ts';
export { automaticShippingProvider, shippingProvider } from './types.ts';
export type * from './types.ts';
export { verifyWebhookSignature } from './webhook.ts';
export type { WebhookSignatureEncoding } from './webhook.ts';

export class KargonomiClient {
  readonly shipments: ShipmentsResource;
  readonly pricing: PricingResource;
  readonly account: AccountResource;
  readonly warehouses: WarehousesResource;
  readonly locations: LocationsResource;
  readonly barcodes: BarcodesResource;
  readonly webhooks: WebhooksResource;

  constructor(options: KargonomiClientOptions) {
    const transport = new Transport(options);
    this.shipments = new ShipmentsResource(transport);
    this.pricing = new PricingResource(transport);
    this.account = new AccountResource(transport);
    this.warehouses = new WarehousesResource(transport);
    this.locations = new LocationsResource(transport);
    this.barcodes = new BarcodesResource(transport);
    this.webhooks = new WebhooksResource(transport);
  }
}
