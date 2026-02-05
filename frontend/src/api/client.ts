export type ApiError = {
  status: number;
  message: string;
};

export type ApiClientOptions = {
  baseUrl: string;
  getToken: () => string | null;
  onUnauthorized?: () => void;
};

export class ApiClient {
  private baseUrl: string;
  private getToken: () => string | null;
  private onUnauthorized?: () => void;

  constructor(opts: ApiClientOptions) {
    this.baseUrl = opts.baseUrl.replace(/\/$/, "");
    this.getToken = opts.getToken;
    this.onUnauthorized = opts.onUnauthorized;
  }

  async request<T>(
    path: string,
    init: RequestInit & { json?: unknown } = {}
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

    const res = await fetch(`${this.baseUrl}${path}`, {
      ...init,
      headers,
      body: init.json !== undefined ? JSON.stringify(init.json) : init.body,
    });

    if (res.status === 401) {
      this.onUnauthorized?.();
    }

    if (!res.ok) {
      const message = await safeReadError(res);
      throw { status: res.status, message } satisfies ApiError;
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
