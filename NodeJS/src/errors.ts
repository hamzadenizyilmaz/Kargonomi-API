export class KargonomiError extends Error {
  override readonly name: string = 'KargonomiError';
  constructor(
    message: string,
    public readonly status: number | null,
    public readonly retryable: boolean,
    public readonly validation?: Readonly<Record<string, readonly string[]>>,
    options?: ErrorOptions
  ) {
    super(message, options);
  }
}

export class AuthenticationError extends KargonomiError {
  override readonly name = 'AuthenticationError';
  constructor(message: string, status: number, options?: ErrorOptions) { super(message, status, false, undefined, options); }
}

export class ValidationError extends KargonomiError {
  override readonly name = 'ValidationError';
  constructor(message: string, validation: Readonly<Record<string, readonly string[]>>, options?: ErrorOptions) { super(message, 422, false, validation, options); }
}

export class NotFoundError extends KargonomiError {
  override readonly name = 'NotFoundError';
  constructor(message: string, options?: ErrorOptions) { super(message, 404, false, undefined, options); }
}

export class RateLimitError extends KargonomiError {
  override readonly name = 'RateLimitError';
  constructor(message: string, public readonly retryAfterSeconds?: number, options?: ErrorOptions) { super(message, 429, true, undefined, options); }
}

export class NetworkError extends KargonomiError {
  override readonly name: string = 'NetworkError';
  constructor(message: string, options?: ErrorOptions) { super(message, null, true, undefined, options); }
}

export class TimeoutError extends NetworkError {
  override readonly name = 'TimeoutError';
}

export class UnexpectedResponseError extends KargonomiError {
  override readonly name = 'UnexpectedResponseError';
  constructor(message: string, status: number | null = null, options?: ErrorOptions) { super(message, status, false, undefined, options); }
}
