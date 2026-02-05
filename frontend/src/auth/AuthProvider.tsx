import React, { createContext, useCallback, useMemo, useState } from "react";
import { ApiClient } from "../api/client";

type AuthState = {
  token: string | null;
  setToken: (token: string | null) => void;
  logout: () => void;
  api: ApiClient;
};

const TOKEN_KEY = "starterkit.token";

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

  // Base URL: configure via Vite env (fallback is your local API)
  const baseUrl =
    import.meta.env.VITE_API_BASE_URL?.toString() ?? "https://localhost:5001";

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
