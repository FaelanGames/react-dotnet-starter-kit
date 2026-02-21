import { describe, expect, beforeEach, afterEach, test, vi } from "vitest";
import { render, waitFor } from "@testing-library/react";
import { act } from "react";
import {
  AuthProvider,
  REFRESH_TOKEN_STORAGE_KEY,
  TOKEN_STORAGE_KEY,
} from "./AuthProvider";
import { useAuth } from "./useAuth";

function renderWithAuthCapture() {
  let latest: ReturnType<typeof useAuth> | null = null;

  function Capture() {
    latest = useAuth();
    return null;
  }

  render(
    <AuthProvider>
      <Capture />
    </AuthProvider>
  );

  return {
    get ctx() {
      if (!latest) {
        throw new Error("Auth context not initialized");
      }
      return latest;
    },
  };
}

describe("AuthProvider", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  test("hydrates tokens from localStorage and persists changes", () => {
    localStorage.setItem(TOKEN_STORAGE_KEY, "stored-token");
    localStorage.setItem(REFRESH_TOKEN_STORAGE_KEY, "stored-refresh");
    const capture = renderWithAuthCapture();

    expect(capture.ctx.token).toBe("stored-token");
    expect(capture.ctx.refreshToken).toBe("stored-refresh");

    act(() => {
      capture.ctx.setAuthTokens({
        accessToken: "next-token",
        refreshToken: "next-refresh",
      });
    });

    expect(localStorage.getItem(TOKEN_STORAGE_KEY)).toBe("next-token");
    expect(localStorage.getItem(REFRESH_TOKEN_STORAGE_KEY)).toBe("next-refresh");

    act(() => {
      capture.ctx.logout();
    });

    expect(localStorage.getItem(TOKEN_STORAGE_KEY)).toBeNull();
    expect(localStorage.getItem(REFRESH_TOKEN_STORAGE_KEY)).toBeNull();
    expect(capture.ctx.token).toBeNull();
  });

  test("refreshes token once when API request returns 401", async () => {
    localStorage.setItem(TOKEN_STORAGE_KEY, "valid-token");
    localStorage.setItem(REFRESH_TOKEN_STORAGE_KEY, "valid-refresh");
    const capture = renderWithAuthCapture();

    let protectedCalls = 0;
    const fetchMock = vi.spyOn(globalThis, "fetch").mockImplementation((input) => {
      const url = typeof input === "string" ? input : input instanceof Request ? input.url : input.toString();
      if (url.includes("/api/auth/refresh")) {
        return Promise.resolve(
          new Response(
            JSON.stringify({
              accessToken: "new-access",
              refreshToken: "new-refresh",
              tokenType: "Bearer",
              expiresInSeconds: 3600,
              refreshTokenExpiresUtc: new Date().toISOString(),
            }),
            { status: 200 }
          )
        );
      }

      if (url.includes("/api/protected")) {
        protectedCalls += 1;
        if (protectedCalls === 1) {
          return Promise.resolve(
            new Response("Unauthorized", { status: 401, statusText: "Unauthorized" })
          );
        }

        return Promise.resolve(
          new Response(JSON.stringify({ ok: true }), {
            status: 200,
            headers: { "Content-Type": "application/json" },
          })
        );
      }

      return Promise.resolve(new Response("ok"));
    });

    await act(async () => {
      const result = await capture.ctx.api.request("/api/protected", { method: "GET" });
      expect(result).toEqual({ ok: true });
    });

    expect(fetchMock).toHaveBeenCalled();
    await waitFor(() => {
      expect(localStorage.getItem(TOKEN_STORAGE_KEY)).toBe("new-access");
      expect(localStorage.getItem(REFRESH_TOKEN_STORAGE_KEY)).toBe("new-refresh");
      expect(capture.ctx.token).toBe("new-access");
    });
  });
});
