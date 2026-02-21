import { act, renderHook, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, test, vi } from "vitest";
import type { ApiClient } from "../../../api/client";
import { login } from "../../../api/auth";
import { useAuth } from "../../../auth/useAuth";
import { useLocation, useNavigate } from "react-router-dom";
import { useLoginForm } from "./useLoginForm";

vi.mock("../../../api/auth", () => ({
  login: vi.fn(),
}));

vi.mock("../../../auth/useAuth", () => ({
  useAuth: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
  const actual = await vi.importActual<typeof import("react-router-dom")>("react-router-dom");
  return {
    ...actual,
    useLocation: vi.fn(),
    useNavigate: vi.fn(),
  };
});

const mockedLogin = vi.mocked(login);
const mockedUseAuth = vi.mocked(useAuth);
const mockedUseLocation = vi.mocked(useLocation);
const mockedUseNavigate = vi.mocked(useNavigate);

describe("useLoginForm", () => {
  const api = {} as ApiClient;

  beforeEach(() => {
    vi.clearAllMocks();
  });

  test("submits login, stores tokens, and redirects to the requested page", async () => {
    const navigate = vi.fn();
    const setAuthTokens = vi.fn();

    mockedUseNavigate.mockReturnValue(navigate);
    mockedUseLocation.mockReturnValue({
      pathname: "/login",
      search: "",
      hash: "",
      key: "test",
      state: { from: "/settings" },
    });
    mockedUseAuth.mockReturnValue({
      token: null,
      refreshToken: null,
      setAuthTokens,
      logout: vi.fn(),
      api,
    });

    mockedLogin.mockResolvedValueOnce({
      accessToken: "new-access",
      refreshToken: "new-refresh",
      tokenType: "Bearer",
      expiresInSeconds: 3600,
      refreshTokenExpiresUtc: new Date().toISOString(),
    });

    const { result } = renderHook(() => useLoginForm());

    act(() => {
      result.current.setEmail("user@example.com");
      result.current.setPassword("Password123!");
    });

    const preventDefault = vi.fn();
    await act(async () => {
      await result.current.onSubmit({ preventDefault } as any);
    });

    expect(preventDefault).toHaveBeenCalledTimes(1);
    expect(mockedLogin).toHaveBeenCalledWith(api, "user@example.com", "Password123!");
    expect(setAuthTokens).toHaveBeenCalledWith({
      accessToken: "new-access",
      refreshToken: "new-refresh",
    });
    expect(navigate).toHaveBeenCalledWith("/settings", { replace: true });
    expect(result.current.error).toBeNull();
    expect(result.current.busy).toBe(false);
  });

  test("sets error when login fails", async () => {
    mockedUseNavigate.mockReturnValue(vi.fn());
    mockedUseLocation.mockReturnValue({
      pathname: "/login",
      search: "",
      hash: "",
      key: "test",
      state: null,
    });
    mockedUseAuth.mockReturnValue({
      token: null,
      refreshToken: null,
      setAuthTokens: vi.fn(),
      logout: vi.fn(),
      api,
    });

    mockedLogin.mockRejectedValueOnce({ message: "Invalid credentials" });

    const { result } = renderHook(() => useLoginForm());

    act(() => {
      result.current.setEmail("user@example.com");
      result.current.setPassword("WrongPassword123!");
    });

    await act(async () => {
      await result.current.onSubmit({ preventDefault: vi.fn() } as any);
    });

    await waitFor(() => {
      expect(result.current.error).toBe("Invalid credentials");
    });
    expect(result.current.busy).toBe(false);
  });
});
