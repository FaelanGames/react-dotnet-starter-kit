import React, { createContext, useCallback, useMemo, useState } from "react";
import { ApiClient } from "../api/client";

type AuthState = {
  token: string | null;
  setToken: (token: string | null) => void;
  logout: () => void;
  api: ApiClient;
};

export const TOKEN_STORAGE_KEY = "starterkit.token";
const TOKEN_KEY = TOKEN_STORAGE_KEY;

export const AuthContext = createContext<AuthState | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setTokenState] = useState<string | null>(() => {
    return localStorage.getItem(TOKEN_KEY);
  });

  const setToken = useCallback((next: string | null) => {
    setTokenState(next);
    if (next) localStorage.setItem(TOKEN_KEY, next);
    else localStorage.removeItem(TOKEN_KEY);
  }, []);

  const logout = useCallback(() => setToken(null), [setToken]);

  // Base URL: configure via Vite env (fallback matches the API launch profile)
  const envApiBase = import.meta.env.VITE_API_BASE_URL?.toString();
  const fallbackApiBase = "https://localhost:7042";
  const baseUrl = envApiBase ?? fallbackApiBase;

  if (!envApiBase && import.meta.env.DEV) {
    console.warn(
      "Missing VITE_API_BASE_URL. Using https://localhost:7042. Create frontend/.env to silence this warning."
    );
  }

  const api = useMemo(() => {
    return new ApiClient({
      baseUrl,
      getToken: () => token,
      onUnauthorized: () => {
        // Token expired or invalid => log out
        setToken(null);
      },
    });
  }, [baseUrl, token, setToken]);

  const value = useMemo(
    () => ({ token, setToken, logout, api }),
    [token, setToken, logout, api]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
