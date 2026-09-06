import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { apiFetch, clearToken, getToken, setToken } from "../api/client";
import type { AuthUser, LoginResponse } from "../types/auth";

type AuthContextValue = {
  user: AuthUser | null;
  isReady: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  hasPermission: (permission: string) => boolean;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    const token = getToken();
    if (!token) {
      setIsReady(true);
      return;
    }

    apiFetch("/api/auth/me")
      .then((u) => setUser(u as unknown as AuthUser))
      .catch(() => {
        clearToken();
        setUser(null);
      })
      .finally(() => setIsReady(true));
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isReady,
      login: async (email, password) => {
        const result = (await apiFetch("/api/auth/login", {
          method: "POST",
          body: JSON.stringify({ email, password }),
        })) as unknown as LoginResponse;
        setToken(result.token);
        setUser(result.user);
      },
      logout: () => {
        clearToken();
        setUser(null);
      },
      hasPermission: (permission) => user?.permissions.includes(permission) ?? false,
    }),
    [user, isReady],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return context;
}
