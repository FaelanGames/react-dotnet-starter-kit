import { describe, expect, test, vi } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import type { ApiClient } from "../../api/client";
import { me } from "../../api/users";
import { useDashboardData } from "./useDashboardData";

vi.mock("../../api/users", () => ({
  me: vi.fn(),
}));

const mockedMe = vi.mocked(me);

describe("useDashboardData", () => {
  test("returns data when api call succeeds", async () => {
    mockedMe.mockResolvedValueOnce({
      id: "user-1",
      email: "user@example.com",
      createdUtc: "2026-02-10T00:00:00Z",
    });

    const api = {} as ApiClient;
    const { result } = renderHook(() => useDashboardData(api));

    await waitFor(() => {
      expect(result.current.data?.email).toBe("user@example.com");
    });
    expect(result.current.error).toBeNull();
  });

  test("returns error when api call fails", async () => {
    mockedMe.mockRejectedValueOnce({ message: "Failed to load user" });

    const api = {} as ApiClient;
    const { result } = renderHook(() => useDashboardData(api));

    await waitFor(() => {
      expect(result.current.error).toBe("Failed to load user");
    });
    expect(result.current.data).toBeNull();
  });
});
