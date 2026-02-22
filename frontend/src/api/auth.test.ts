import { describe, expect, test, vi } from "vitest";
import { login, logout, refresh, register } from "./auth";
import type { ApiClient } from "./client";

describe("auth api", () => {
  test("register calls expected endpoint", async () => {
    const request = vi.fn().mockResolvedValue({ accessToken: "a" });
    const api = { request } as unknown as ApiClient;

    await register(api, "user@example.com", "Password123!");

    expect(request).toHaveBeenCalledWith("/api/auth/register", {
      method: "POST",
      json: { email: "user@example.com", password: "Password123!" },
    });
  });

  test("login calls expected endpoint", async () => {
    const request = vi.fn().mockResolvedValue({ accessToken: "a" });
    const api = { request } as unknown as ApiClient;

    await login(api, "user@example.com", "Password123!");

    expect(request).toHaveBeenCalledWith("/api/auth/login", {
      method: "POST",
      json: { email: "user@example.com", password: "Password123!" },
    });
  });

  test("refresh calls expected endpoint", async () => {
    const request = vi.fn().mockResolvedValue({ accessToken: "a" });
    const api = { request } as unknown as ApiClient;

    await refresh(api, "refresh-token");

    expect(request).toHaveBeenCalledWith("/api/auth/refresh", {
      method: "POST",
      json: { refreshToken: "refresh-token" },
    });
  });

  test("logout calls expected endpoint", async () => {
    const request = vi.fn().mockResolvedValue(undefined);
    const api = { request } as unknown as ApiClient;

    await logout(api, "refresh-token");

    expect(request).toHaveBeenCalledWith("/api/auth/logout", {
      method: "POST",
      json: { refreshToken: "refresh-token" },
    });
  });
});
