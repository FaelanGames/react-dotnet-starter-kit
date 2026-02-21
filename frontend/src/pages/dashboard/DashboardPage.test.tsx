import { describe, expect, test, vi } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";
import type { ContextType } from "react";
import { AuthContext } from "../../auth/AuthProvider";
import { DashboardPage } from "./DashboardPage";
import type { ApiClient } from "../../api/client";
import { useDashboardData } from "./useDashboardData";

vi.mock("./useDashboardData", () => ({
  useDashboardData: vi.fn(),
}));

const mockedUseDashboardData = vi.mocked(useDashboardData);
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

  return {
    ...render(
      <AuthContext.Provider value={authValue}>
        <DashboardPage />
      </AuthContext.Provider>
    ),
    authValue,
  };
}

describe("DashboardPage", () => {
  test("renders user info when data exists", () => {
    mockedUseDashboardData.mockReturnValue({
      data: {
        id: "user-1",
        email: "user@example.com",
        createdUtc: "2026-02-10T00:00:00Z",
      },
      error: null,
    });

    renderWithAuth();

    expect(screen.getByText(/user@example.com/)).toBeInTheDocument();
  });

  test("shows error when hook returns an error", () => {
    mockedUseDashboardData.mockReturnValue({ data: null, error: "Failed to load user" });

    renderWithAuth();

    expect(screen.getByText("Failed to load user")).toBeInTheDocument();
  });

  test("calls logout when logout button is clicked", () => {
    mockedUseDashboardData.mockReturnValue({ data: null, error: null });

    const { authValue } = renderWithAuth();

    fireEvent.click(screen.getByRole("button", { name: /logout/i }));

    expect(authValue.logout).toHaveBeenCalledTimes(1);
  });
});
