export class ApiError extends Error {
  status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

export type ApiClientOptions = {
  baseUrl: string;
  getToken: () => string | null;
  onUnauthorized?: () => Promise<boolean> | boolean;
};

export class ApiClient {
  private baseUrl: string;
  private getToken: () => string | null;
  private onUnauthorized?: () => Promise<boolean> | boolean;

  constructor(opts: ApiClientOptions) {
    this.baseUrl = opts.baseUrl.replace(/\/$/, "");
    this.getToken = opts.getToken;
    this.onUnauthorized = opts.onUnauthorized;
  }

  async request<T>(
    path: string,
    init: RequestInit & { json?: unknown } = {},
    retrying = false
  ): Promise<T> {
    const headers = new Headers(init.headers);

    // Default headers
    if (!headers.has("Accept")) headers.set("Accept", "application/json");

    // JSON body handling
    if (init.json !== undefined) {
      headers.set("Content-Type", "application/json");
    }

    // Auth header
    const token = this.getToken();
    if (token) headers.set("Authorization", `Bearer ${token}`);

    const body = init.json !== undefined ? JSON.stringify(init.json) : init.body;
    const requestInit: RequestInit = {
      ...init,
      headers,
      body,
    };
    delete (requestInit as { json?: unknown }).json;

    const res = await fetch(`${this.baseUrl}${path}`, requestInit);

    if (res.status === 401 && !retrying && this.onUnauthorized) {
      const recovered = await this.onUnauthorized();
      if (recovered) {
        return this.request<T>(path, init, true);
      }
    }

    if (!res.ok) {
      const message = await safeReadError(res);
      throw new ApiError(res.status, message);
    }

    // Handle empty responses (just in case)
    const text = await res.text();
    if (!text) return undefined as T;

    return JSON.parse(text) as T;
  }
}

async function safeReadError(res: Response): Promise<string> {
  try {
    const text = await res.text();
    if (!text) return res.statusText || "Request failed";
    return text;
  } catch {
    return res.statusText || "Request failed";
  }
}
