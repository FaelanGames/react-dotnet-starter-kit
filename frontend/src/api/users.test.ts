import { me } from "./users";
import type { ApiClient } from "./client";
import { describe, expect, test, vi } from "vitest";

describe("users api", () => {
  test("me calls expected endpoint", async () => {
    const request = vi.fn().mockResolvedValue({ id: "u1", email: "u@example.com", createdUtc: "2026-02-22T00:00:00Z" });
    const api = { request } as unknown as ApiClient;

    const result = await me(api);

    expect(result.id).toBe("u1");
    expect(request).toHaveBeenCalledWith("/api/users/me", { method: "GET" });
  });
});
