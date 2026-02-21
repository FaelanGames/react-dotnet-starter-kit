import { act, renderHook, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, test, vi } from "vitest";
import type { ApiClient } from "../../../api/client";
import { register } from "../../../api/auth";
import { useAuth } from "../../../auth/useAuth";
import { useNavigate } from "react-router-dom";
import { useRegisterForm } from "./useRegisterForm";

vi.mock("../../../api/auth", () => ({
  register: vi.fn(),
}));

vi.mock("../../../auth/useAuth", () => ({
  useAuth: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
  const actual = await vi.importActual<typeof import("react-router-dom")>("react-router-dom");
  return {
    ...actual,
    useNavigate: vi.fn(),
  };
});

const mockedRegister = vi.mocked(register);
const mockedUseAuth = vi.mocked(useAuth);
const mockedUseNavigate = vi.mocked(useNavigate);

describe("useRegisterForm", () => {
  const api = {} as ApiClient;

  beforeEach(() => {
    vi.clearAllMocks();
  });

  test("submits register, stores tokens, and redirects to dashboard", async () => {
    const navigate = vi.fn();
    const setAuthTokens = vi.fn();

    mockedUseNavigate.mockReturnValue(navigate);
    mockedUseAuth.mockReturnValue({
      token: null,
      refreshToken: null,
      setAuthTokens,
      logout: vi.fn(),
      api,
    });

    mockedRegister.mockResolvedValueOnce({
      accessToken: "new-access",
      refreshToken: "new-refresh",
      tokenType: "Bearer",
      expiresInSeconds: 3600,
      refreshTokenExpiresUtc: new Date().toISOString(),
    });

    const { result } = renderHook(() => useRegisterForm());

    act(() => {
      result.current.setEmail("user@example.com");
      result.current.setPassword("Password123!");
    });

    const preventDefault = vi.fn();
    await act(async () => {
      await result.current.onSubmit({ preventDefault } as any);
    });

    expect(preventDefault).toHaveBeenCalledTimes(1);
    expect(mockedRegister).toHaveBeenCalledWith(api, "user@example.com", "Password123!");
    expect(setAuthTokens).toHaveBeenCalledWith({
      accessToken: "new-access",
      refreshToken: "new-refresh",
    });
    expect(navigate).toHaveBeenCalledWith("/dashboard", { replace: true });
    expect(result.current.error).toBeNull();
    expect(result.current.busy).toBe(false);
  });

  test("sets friendly duplicate-email error on 409", async () => {
    mockedUseNavigate.mockReturnValue(vi.fn());
    mockedUseAuth.mockReturnValue({
      token: null,
      refreshToken: null,
      setAuthTokens: vi.fn(),
      logout: vi.fn(),
      api,
    });
    mockedRegister.mockRejectedValueOnce({ status: 409, message: "Conflict" });

    const { result } = renderHook(() => useRegisterForm());

    await act(async () => {
      await result.current.onSubmit({ preventDefault: vi.fn() } as any);
    });

    await waitFor(() => {
      expect(result.current.error).toBe("Email is already registered.");
    });
    expect(result.current.busy).toBe(false);
  });

  test("sets fallback error when register fails", async () => {
    mockedUseNavigate.mockReturnValue(vi.fn());
    mockedUseAuth.mockReturnValue({
      token: null,
      refreshToken: null,
      setAuthTokens: vi.fn(),
      logout: vi.fn(),
      api,
    });
    mockedRegister.mockRejectedValueOnce({});

    const { result } = renderHook(() => useRegisterForm());

    await act(async () => {
      await result.current.onSubmit({ preventDefault: vi.fn() } as any);
    });

    await waitFor(() => {
      expect(result.current.error).toBe("Registration failed");
    });
  });
});
