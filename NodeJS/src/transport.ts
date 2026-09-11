import { AuthenticationError, KargonomiError, NetworkError, NotFoundError, RateLimitError, TimeoutError, UnexpectedResponseError, ValidationError } from './errors.ts';
import { configuredBaseUrl } from './settings.ts';
import type { KargonomiClientOptions } from './types.ts';

export class Transport {
  readonly #token: string;
  readonly #baseUrl: URL;
  readonly #timeoutMs: number;
  readonly #fetch: typeof globalThis.fetch;
  readonly #maxRetries: number;
  readonly #retryDelayMs: number;

  constructor(options: KargonomiClientOptions) {
    const token = options.apiToken.trim();
    if (token.length === 0) throw new TypeError('Kargonomi API token is required.');
    const baseUrl = new URL(options.baseUrl ?? configuredBaseUrl);
    if (baseUrl.protocol !== 'https:') throw new TypeError('Kargonomi baseUrl must use HTTPS.');
    const timeoutMs = options.timeoutMs ?? 30_000;
    if (!Number.isSafeInteger(timeoutMs) || timeoutMs <= 0) throw new RangeError('timeoutMs must be a positive integer.');
    const maxRetries = options.maxRetries ?? 2;
    const retryDelayMs = options.retryDelayMs ?? 250;
    if (!Number.isSafeInteger(maxRetries) || maxRetries < 0 || maxRetries > 5) throw new RangeError('maxRetries must be an integer from 0 to 5.');
    if (!Number.isSafeInteger(retryDelayMs) || retryDelayMs < 0 || retryDelayMs > 60_000) throw new RangeError('retryDelayMs must be an integer from 0 to 60000.');
    this.#token = token;
    this.#baseUrl = baseUrl;
    this.#timeoutMs = timeoutMs;
    this.#fetch = options.fetch ?? globalThis.fetch;
    this.#maxRetries = maxRetries;
    this.#retryDelayMs = retryDelayMs;
  }

  async json<T>(method: string, path: string, decode: (value: unknown) => T, body?: unknown, signal?: AbortSignal): Promise<T> {
    const headers = new Headers({ accept: 'application/json', authorization: `Bearer ${this.#token}`, 'user-agent': '@kargonomi/client/2.5.0-Enterprise' });
    let requestBody: BodyInit | undefined;
    if (body instanceof FormData) requestBody = body;
    else if (body !== undefined) { headers.set('content-type', 'application/json'); requestBody = JSON.stringify(body); }
    const response = await this.#send(new Request(this.#url(path), { method, headers, body: requestBody ?? null, redirect: 'manual', signal: this.#signal(signal) }));
    if (response.status === 204) throw new UnexpectedResponseError('Kargonomi returned an empty response where JSON was required.', 204);
    const text = await response.text();
    let value: unknown;
    try { value = JSON.parse(text) as unknown; }
    catch (cause) { throw new UnexpectedResponseError('Kargonomi returned malformed JSON.', response.status, { cause }); }
    return decode(value);
  }

  async empty(method: string, path: string, signal?: AbortSignal): Promise<void> {
    const headers = new Headers({ accept: 'application/json', authorization: `Bearer ${this.#token}`, 'user-agent': '@kargonomi/client/2.5.0-Enterprise' });
    await this.#send(new Request(this.#url(path), { method, headers, redirect: 'manual', signal: this.#signal(signal) }));
  }

  async #send(request: Request): Promise<Response> {
    let response: Response | undefined;
    for (let attempt = 0; attempt <= this.#maxRetries; attempt += 1) {
      try { response = await this.#fetch(new Request(request)); }
      catch (cause) {
        if (cause instanceof DOMException && (cause.name === 'AbortError' || cause.name === 'TimeoutError')) throw new TimeoutError('Kargonomi request timed out or was cancelled.', { cause });
        if (request.method !== 'GET' || attempt >= this.#maxRetries) throw new NetworkError('Kargonomi could not be reached.', { cause });
        await this.#delay(attempt);
        continue;
      }
      if (response.ok) return response;
      const retryable = response.status === 408 || response.status === 429 || response.status >= 500;
      if (request.method === 'GET' && retryable && attempt < this.#maxRetries) {
        await this.#delay(attempt, response);
        continue;
      }
      break;
    }
    if (response === undefined) throw new NetworkError('Kargonomi could not be reached.');
    let validation: Readonly<Record<string, readonly string[]>> | undefined;
    if (response.status === 422) {
      try {
        const value = await response.clone().json() as unknown;
        const errors = (typeof value === 'object' && value !== null) ? (value as Record<string, unknown>).errors : undefined;
        if (typeof errors === 'object' && errors !== null && !Array.isArray(errors)) validation = Object.fromEntries(Object.entries(errors).map(([key, messages]) => [key, Array.isArray(messages) ? messages.filter((item): item is string => typeof item === 'string') : ['Invalid value.']]));
      } catch { validation = undefined; }
    }
    const message = response.status === 401 || response.status === 403 ? 'Kargonomi authentication failed.' : response.status === 404 ? 'Kargonomi resource was not found.' : response.status === 422 ? 'Kargonomi rejected one or more request fields.' : `Kargonomi request failed with HTTP ${String(response.status)}.`;
    if (response.status === 401 || response.status === 403) throw new AuthenticationError(message, response.status);
    if (response.status === 404) throw new NotFoundError(message);
    if (response.status === 422) throw new ValidationError(message, validation ?? {});
    if (response.status === 429) {
      const header = response.headers.get('retry-after');
      throw new RateLimitError(message, header !== null && /^\d+$/u.test(header) ? Number(header) : undefined);
    }
    throw new KargonomiError(message, response.status, response.status === 408 || response.status >= 500, validation);
  }

  async #delay(attempt: number, response?: Response): Promise<void> {
    const retryAfter = response?.headers.get('retry-after');
    const serverDelay = retryAfter !== undefined && retryAfter !== null && /^\d+$/u.test(retryAfter) ? Number(retryAfter) * 1000 : 0;
    const milliseconds = Math.min(60_000, Math.max(serverDelay, this.#retryDelayMs * (2 ** attempt)));
    if (milliseconds > 0) await new Promise((resolve) => setTimeout(resolve, milliseconds));
  }

  #url(path: string): URL {
    const url = new URL(path.replace(/^\//, ''), this.#baseUrl);
    if (url.origin !== this.#baseUrl.origin) throw new TypeError('Endpoint escaped the configured Kargonomi origin.');
    return url;
  }

  #signal(signal?: AbortSignal): AbortSignal {
    const timeout = AbortSignal.timeout(this.#timeoutMs);
    return signal === undefined ? timeout : AbortSignal.any([signal, timeout]);
  }
}
