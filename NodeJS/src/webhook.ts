import { createHmac, timingSafeEqual } from 'node:crypto';

export type WebhookSignatureEncoding = 'hex' | 'base64';

export function verifyWebhookSignature(rawBody: Uint8Array, signature: string, secret: Uint8Array, encoding: WebhookSignatureEncoding): boolean {
  if (signature.trim() === '' || secret.byteLength === 0) return false;
  let supplied: Buffer;
  try { supplied = Buffer.from(signature, encoding); }
  catch { return false; }
  if ((encoding === 'hex' && !/^[a-f\d]+$/iu.test(signature)) || supplied.length !== 32) return false;
  const expected = createHmac('sha256', secret).update(rawBody).digest();
  return supplied.length === expected.length && timingSafeEqual(supplied, expected);
}
