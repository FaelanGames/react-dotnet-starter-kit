import React, {
  createContext,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import { ApiClient } from "../api/client";
import { refresh as refreshAuth, logout as revokeRefreshToken } from "../api/auth";

type AuthState = {
  token: string | null;
  refreshToken: string | null;
  setAuthTokens: (tokens: AuthTokens) => void;
  logout: () => void;
  api: ApiClient;
};

export const TOKEN_STORAGE_KEY = "starterkit.token";
export const REFRESH_TOKEN_STORAGE_KEY = "starterkit.refreshToken";

type AuthTokens = {
  accessToken: string | null;
  refreshToken: string | null;
};

export const AuthContext = createContext<AuthState | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setTokenState] = useState<string | null>(() => {
    return localStorage.getItem(TOKEN_STORAGE_KEY);
  });
  const [refreshToken, setRefreshTokenState] = useState<string | null>(() => {
    return localStorage.getItem(REFRESH_TOKEN_STORAGE_KEY);
  });
  const refreshTokenRef = useRef<string | null>(refreshToken);

  useEffect(() => {
    refreshTokenRef.current = refreshToken;
  }, [refreshToken]);

  const setAuthTokens = useCallback((next: AuthTokens) => {
    setTokenState(next.accessToken);
    setRefreshTokenState(next.refreshToken);

    if (next.accessToken) localStorage.setItem(TOKEN_STORAGE_KEY, next.accessToken);
    else localStorage.removeItem(TOKEN_STORAGE_KEY);

    if (next.refreshToken)
      localStorage.setItem(REFRESH_TOKEN_STORAGE_KEY, next.refreshToken);
    else localStorage.removeItem(REFRESH_TOKEN_STORAGE_KEY);
  }, []);

  const clearTokens = useCallback(() => {
    setAuthTokens({ accessToken: null, refreshToken: null });
  }, [setAuthTokens]);

  // Base URL: configure via Vite env (fallback matches the API launch profile)
  const envApiBase = import.meta.env.VITE_API_BASE_URL?.toString();
  const fallbackApiBase = "https://localhost:7042";
  const baseUrl = envApiBase ?? fallbackApiBase;

  if (!envApiBase && import.meta.env.DEV) {
    console.warn(
      "Missing VITE_API_BASE_URL. Using https://localhost:7042. Create frontend/.env to silence this warning."
    );
  }

  const authlessApi = useMemo(() => {
    return new ApiClient({
      baseUrl,
      getToken: () => null,
    });
  }, [baseUrl]);

  const handleUnauthorized = useCallback(async () => {
    const currentRefresh = refreshTokenRef.current;
    if (!currentRefresh) {
      clearTokens();
      return false;
    }

    try {
      const res = await refreshAuth(authlessApi, currentRefresh);
      setAuthTokens({
        accessToken: res.accessToken,
        refreshToken: res.refreshToken,
      });
      return true;
    } catch {
      clearTokens();
      return false;
    }
  }, [authlessApi, setAuthTokens, clearTokens]);

  const api = useMemo(() => {
    return new ApiClient({
      baseUrl,
      getToken: () => token,
      onUnauthorized: handleUnauthorized,
    });
  }, [baseUrl, token, handleUnauthorized]);

  const logout = useCallback(() => {
    const currentRefresh = refreshTokenRef.current;
    if (currentRefresh) {
      revokeRefreshToken(authlessApi, currentRefresh).catch(() => {
        // best-effort revoke
      });
    }
    clearTokens();
  }, [authlessApi, clearTokens]);

  const value = useMemo(
    () => ({ token, refreshToken, setAuthTokens, logout, api }),
    [token, refreshToken, setAuthTokens, logout, api]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
