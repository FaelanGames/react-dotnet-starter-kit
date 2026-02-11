import { describe, expect, beforeEach, afterEach, test, vi } from "vitest";
import { render, waitFor } from "@testing-library/react";
import { act } from "react";
import { AuthProvider, TOKEN_STORAGE_KEY } from "../auth/AuthProvider";
import { useAuth } from "../auth/useAuth";

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

  test("hydrates token from localStorage and persists changes", () => {
    localStorage.setItem(TOKEN_STORAGE_KEY, "stored-token");
    const capture = renderWithAuthCapture();

    expect(capture.ctx.token).toBe("stored-token");

    act(() => {
      capture.ctx.setToken("next-token");
    });

    expect(localStorage.getItem(TOKEN_STORAGE_KEY)).toBe("next-token");

    act(() => {
      capture.ctx.logout();
    });

    expect(localStorage.getItem(TOKEN_STORAGE_KEY)).toBeNull();
    expect(capture.ctx.token).toBeNull();
  });

  test("clears token when API request returns 401", async () => {
    localStorage.setItem(TOKEN_STORAGE_KEY, "valid-token");
    const capture = renderWithAuthCapture();

    const fetchMock = vi
      .spyOn(globalThis, "fetch")
      .mockResolvedValue(
        new Response("Unauthorized", { status: 401, statusText: "Unauthorized" })
      );

    await act(async () => {
      await expect(
        capture.ctx.api.request("/api/protected", { method: "GET" })
      ).rejects.toMatchObject({ status: 401 });
    });

    expect(fetchMock).toHaveBeenCalled();
    await waitFor(() => {
      expect(localStorage.getItem(TOKEN_STORAGE_KEY)).toBeNull();
      expect(capture.ctx.token).toBeNull();
    });
  });
});
