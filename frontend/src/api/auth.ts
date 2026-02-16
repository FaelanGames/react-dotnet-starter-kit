import type { ApiClient } from "./client";

export type AuthResponse = {
  accessToken: string;
  refreshToken: string;
  tokenType: string;
  expiresInSeconds: number;
  refreshTokenExpiresUtc: string;
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

export async function refresh(
  api: ApiClient,
  refreshToken: string
): Promise<AuthResponse> {
  return api.request<AuthResponse>("/api/auth/refresh", {
    method: "POST",
    json: { refreshToken },
  });
}

export async function logout(api: ApiClient, refreshToken: string): Promise<void> {
  await api.request("/api/auth/logout", {
    method: "POST",
    json: { refreshToken },
  });
}
