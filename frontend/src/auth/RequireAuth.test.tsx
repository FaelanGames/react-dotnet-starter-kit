import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { RequireAuth } from "./RequireAuth";
import { AuthContext } from "./AuthProvider";
import type { ApiClient } from "../api/client";
import type { ContextType } from "react";

function renderWithAuth(path: string, token: string | null) {
  const value: NonNullable<ContextType<typeof AuthContext>> = {
    token,
    refreshToken: null,
    setAuthTokens: vi.fn(),
    logout: vi.fn(),
    api: {} as ApiClient,
  };

  return render(
    <AuthContext.Provider value={value}>
      <MemoryRouter initialEntries={[path]}>
        <Routes>
          <Route
            path="/dashboard"
            element={
              <RequireAuth>
                <h2>Protected Content</h2>
              </RequireAuth>
            }
          />
          <Route path="/login" element={<h2>Login</h2>} />
        </Routes>
      </MemoryRouter>
    </AuthContext.Provider>
  );
}

describe("RequireAuth", () => {
  test("renders children when token exists", () => {
    renderWithAuth("/dashboard", "token");
    expect(screen.getByRole("heading", { name: /protected content/i })).toBeInTheDocument();
  });

  test("redirects to login when token is missing", () => {
    renderWithAuth("/dashboard", null);
    expect(screen.getByRole("heading", { name: /login/i })).toBeInTheDocument();
  });
});
