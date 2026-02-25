import { afterEach, describe, expect, test, vi } from "vitest";
import { ApiClient } from "./client";

describe("ApiClient", () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  test("sends auth and json headers and parses response", async () => {
    const fetchMock = vi.spyOn(globalThis, "fetch").mockResolvedValueOnce(
      new Response(JSON.stringify({ ok: true }), {
        status: 200,
        headers: { "Content-Type": "application/json" },
      })
    );

    const client = new ApiClient({
      baseUrl: "https://api.example.com/",
      getToken: () => "jwt",
    });

    const result = await client.request<{ ok: boolean }>("/ping", {
      method: "POST",
      json: { value: 1 },
    });

    expect(result).toEqual({ ok: true });
    expect(fetchMock).toHaveBeenCalledTimes(1);

    const [, init] = fetchMock.mock.calls[0];
    const headers = init?.headers as Headers;
    expect(headers.get("Accept")).toBe("application/json");
    expect(headers.get("Content-Type")).toBe("application/json");
    expect(headers.get("Authorization")).toBe("Bearer jwt");
    expect(init?.body).toBe(JSON.stringify({ value: 1 }));
  });

  test("retries once after 401 when unauthorized handler recovers", async () => {
    const fetchMock = vi.spyOn(globalThis, "fetch")
      .mockResolvedValueOnce(new Response("Unauthorized", { status: 401, statusText: "Unauthorized" }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ ok: true }), { status: 200 }));

    const onUnauthorized = vi.fn().mockResolvedValue(true);
    const client = new ApiClient({
      baseUrl: "https://api.example.com",
      getToken: () => null,
      onUnauthorized,
    });

    const result = await client.request<{ ok: boolean }>("/secure", { method: "GET" });

    expect(result).toEqual({ ok: true });
    expect(onUnauthorized).toHaveBeenCalledTimes(1);
    expect(fetchMock).toHaveBeenCalledTimes(2);
  });

  test("throws ApiError when response is not ok and no recovery", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValueOnce(new Response("boom", { status: 500, statusText: "Server Error" }));

    const client = new ApiClient({
      baseUrl: "https://api.example.com",
      getToken: () => null,
      onUnauthorized: () => false,
    });

    await expect(client.request("/fail", { method: "GET" })).rejects.toMatchObject({
      name: "ApiError",
      status: 500,
      message: "boom",
    });
  });

  test("returns undefined for empty successful responses", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValueOnce(new Response(null, { status: 204 }));

    const client = new ApiClient({
      baseUrl: "https://api.example.com",
      getToken: () => null,
    });

    const result = await client.request<undefined>("/empty", { method: "POST" });
    expect(result).toBeUndefined();
  });

  test("uses statusText fallback when error body is empty", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValueOnce(new Response("", { status: 400, statusText: "Bad Request" }));

    const client = new ApiClient({
      baseUrl: "https://api.example.com",
      getToken: () => null,
    });

    await expect(client.request("/bad", { method: "GET" })).rejects.toMatchObject({
      status: 400,
      message: "Bad Request",
    });
  });
});
