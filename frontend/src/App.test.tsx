import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import App from "./App";
import { AuthContext } from "./auth/AuthProvider";
import type { ApiClient } from "./api/client";
import type { ContextType } from "react";

vi.mock("./pages/dashboard/DashboardPage", () => ({
  DashboardPage: () => <h1>Dashboard</h1>,
}));

function renderApp(path: string, auth?: Partial<NonNullable<ContextType<typeof AuthContext>>>) {
  const value: NonNullable<ContextType<typeof AuthContext>> = {
    token: null,
    refreshToken: null,
    setAuthTokens: vi.fn(),
    logout: vi.fn(),
    api: {} as ApiClient,
    ...auth,
  };

  return render(
    <AuthContext.Provider value={value}>
      <MemoryRouter initialEntries={[path]}>
        <App />
      </MemoryRouter>
    </AuthContext.Provider>
  );
}

describe("App routes", () => {
  test("shows Login page when opening /login", () => {
    renderApp("/login");
    expect(screen.getByRole("heading", { name: /login/i })).toBeInTheDocument();
  });

  test("shows Register page when opening /register", () => {
    renderApp("/register");
    expect(screen.getByRole("heading", { name: /register/i })).toBeInTheDocument();
  });

  test("redirects unknown routes to dashboard/login flow", () => {
    renderApp("/does-not-exist");
    expect(screen.getByRole("heading", { name: /login/i })).toBeInTheDocument();
  });

  test("shows Dashboard when user is authenticated", () => {
    renderApp("/dashboard", { token: "jwt-token" });
    expect(screen.getByRole("heading", { name: /dashboard/i })).toBeInTheDocument();
  });
});
