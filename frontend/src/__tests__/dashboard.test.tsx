import { describe, expect, test, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import type { ContextType } from "react";
import { AuthContext } from "../auth/AuthProvider";
import { DashboardPage } from "../pages/DashboardPage";
import type { ApiClient } from "../api/client";
import { me } from "../api/users";

vi.mock("../api/users", () => ({
  me: vi.fn(),
}));

const mockedMe = vi.mocked(me);
type AuthValue = NonNullable<ContextType<typeof AuthContext>>;

function renderWithAuth(value?: Partial<AuthValue>) {
  const authValue: AuthValue = {
    token: "token",
    refreshToken: "refresh",
    setAuthTokens: vi.fn(),
    logout: vi.fn(),
    api: {} as ApiClient,
    ...value,
  };

  return render(
    <AuthContext.Provider value={authValue}>
      <DashboardPage />
    </AuthContext.Provider>
  );
}

describe("DashboardPage", () => {
  test("renders user info when API succeeds", async () => {
    mockedMe.mockResolvedValueOnce({
      id: "user-1",
      email: "user@example.com",
      createdUtc: "2026-02-10T00:00:00Z",
    });

    renderWithAuth();

    const jsonBlock = await screen.findByText(/user@example.com/);
    expect(jsonBlock).toBeInTheDocument();
    expect(mockedMe).toHaveBeenCalledTimes(1);
  });

  test("shows error when API call fails", async () => {
    mockedMe.mockRejectedValueOnce({ message: "Failed to load user" });

    renderWithAuth();

    await waitFor(() =>
      expect(screen.getByText("Failed to load user")).toBeInTheDocument()
    );
  });
});
