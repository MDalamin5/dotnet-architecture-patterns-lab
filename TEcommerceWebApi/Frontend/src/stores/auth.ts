import { create } from "zustand";
import { persist } from "zustand/middleware";

export interface AuthUser {
  userId: string;
  email: string;
  fullName: string;
  role: string;
  tenantId?: string;
  permissions: string[];
}

interface AuthState {
  token: string | null;
  user: AuthUser | null;
  setAuth: (token: string, user: AuthUser) => void;
  logout: () => void;
  hasPermission: (permissionCode: string) => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      token: null,
      user: null,
      setAuth: (token, user) => set({ token, user }),
      logout: () => set({ token: null, user: null }),
      hasPermission: (permissionCode) => get().user?.permissions.includes(permissionCode) ?? false,
    }),
    {
      name: "byv-auth",
      partialize: (state) => ({ token: state.token, user: state.user }),
      // Hydrated manually after mount (see stores/hydration.ts) so SSR and the first client render match.
      skipHydration: true,
    },
  ),
);

export const isMerchant = (user: AuthUser | null) => user?.role === "Admin";
