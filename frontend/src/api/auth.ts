import type { ApiClient } from "./client";

export type AuthResponse = {
  accessToken: string;
  tokenType: string;
  expiresInSeconds: number;
};

export async function register(
  api: ApiClient,
  email: string,
  password: string
): Promise<AuthResponse> {
  return api.request<AuthResponse>("/api/auth/register", {
    method: "POST",
    json: { email, password },
  });
}

export async function login(
  api: ApiClient,
  email: string,
  password: string
): Promise<AuthResponse> {
  return api.request<AuthResponse>("/api/auth/login", {
    method: "POST",
    json: { email, password },
  });
}
