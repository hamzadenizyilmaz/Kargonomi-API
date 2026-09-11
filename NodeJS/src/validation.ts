import type { ShipmentPatchInput, ShipmentWriteInput, WarehouseCreateInput, WebhookWriteInput } from './types.ts';
import { isIP } from 'node:net';

export function validateShipmentWrite(input: ShipmentWriteInput): void {
  if (input.buyerName.trim().length < 5 || input.buyerName.trim().split(/\s+/u).length < 2) throw new TypeError('buyerName must contain at least two words and five characters.');
  if (!/^\d{10}$/u.test(input.buyerPhone)) throw new TypeError('buyerPhone must contain exactly 10 digits.');
  if (input.buyerAddress.trim().length < 10 || input.buyerAddress.length > 512) throw new TypeError('buyerAddress must contain 10-512 characters.');
  if (input.buyerStateId <= 0 || input.buyerCityId <= 0) throw new RangeError('buyer state and city IDs must be positive.');
  if (input.packages.length === 0 || input.packages.some(({ desi }) => !/^\d+(?:\.\d+)?$/u.test(desi) || Number(desi) <= 0)) throw new TypeError('At least one package with positive desi is required.');
  const manualKeys: readonly (keyof ShipmentWriteInput)[] = ['senderName', 'senderEmail', 'senderTaxNumber', 'senderTaxPlace', 'senderPhone', 'senderAddress', 'senderStateId', 'senderCityId'];
  if (input.warehouseId !== undefined && (!Number.isSafeInteger(input.warehouseId) || input.warehouseId <= 0)) throw new RangeError('warehouseId must be a positive safe integer.');
  if (input.warehouseId !== undefined && manualKeys.some((key) => input[key] !== undefined)) throw new TypeError('warehouseId cannot be combined with manual sender fields.');
  if (input.warehouseId === undefined && (input.senderName === undefined || input.senderPhone === undefined || input.senderAddress === undefined || input.senderStateId === undefined || input.senderCityId === undefined)) throw new TypeError('Manual sender fields are required without warehouseId.');
}

export function validatePatch(input: ShipmentPatchInput): void {
  if (Object.keys(input).length === 0) throw new TypeError('At least one PATCH field is required.');
  if (input.buyerPhone !== undefined && input.buyerPhone !== null && !/^\d{10}$/u.test(input.buyerPhone)) throw new TypeError('buyerPhone must contain exactly 10 digits.');
  if (Array.isArray(input.packages) && input.packages.length === 0) throw new TypeError('packages cannot be empty.');
}

export function validateWarehouse(input: WarehouseCreateInput): void {
  if (input.name.trim().length < 2 || input.address.trim().length < 10) throw new TypeError('Warehouse name and address are required.');
  if (!/^\d{10}$/u.test(input.contactPhone) || !/^\d{10,11}$/u.test(input.taxNumber)) throw new TypeError('Warehouse phone or tax number is invalid.');
  if (input.stateId <= 0 || input.cityId <= 0) throw new RangeError('Warehouse state and city IDs must be positive.');
}

export function validateWebhook(input: WebhookWriteInput): void {
  if (input.name.trim().length < 2 || input.eventType.trim().length === 0) throw new TypeError('Webhook name and eventType are required.');
  const url = new URL(input.url);
  if (url.protocol !== 'https:' || url.username !== '' || url.password !== '' || !isPublicHostname(url.hostname)) throw new TypeError('Webhook URL must target a public HTTPS host without user information.');
}

function isPublicHostname(value: string): boolean {
  const hostname = value.replace(/^\[|\]$/gu, '').toLocaleLowerCase('en-US');
  if (hostname === 'localhost' || hostname.endsWith('.localhost') || hostname.endsWith('.local') || hostname.endsWith('.internal')) return false;
  const version = isIP(hostname);
  if (version === 0) return hostname.includes('.');
  if (version === 6) return !(hostname === '::' || hostname === '::1' || hostname.startsWith('fc') || hostname.startsWith('fd') || /^fe[89ab]/u.test(hostname));
  const [first = 0, second = 0] = hostname.split('.').map(Number);
  return !(first === 10 || first === 0 || first === 127 || (first === 169 && second === 254) || (first === 172 && second >= 16 && second <= 31) || (first === 192 && second === 168) || first >= 224);
}
