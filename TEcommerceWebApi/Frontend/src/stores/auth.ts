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

const createAuthStore = (storageKey: string) =>
  create<AuthState>()(
    persist(
      (set, get) => ({
        token: null,
        user: null,
        setAuth: (token, user) => set({ token, user }),
        logout: () => set({ token: null, user: null }),
        hasPermission: (permissionCode) => get().user?.permissions.includes(permissionCode) ?? false,
      }),
      {
        name: storageKey,
        partialize: (state) => ({ token: state.token, user: state.user }),
        // Hydrated manually after mount (see stores/hydration.ts) so SSR and the first client render match.
        skipHydration: true,
      },
    ),
  );

/**
 * Two independent sessions share one origin on localhost, so they get separate storage slots:
 * a merchant signed in to /admin and a customer signed in to /store/<tenant> never overwrite each other.
 * (In production the portals are different origins anyway.)
 */
/** Storefront customer session. */
export const useAuthStore = createAuthStore("byv-auth");
/** Merchant (store owner) session used by /admin. */
export const useMerchantAuthStore = createAuthStore("byv-merchant-auth");

export const isMerchant = (user: AuthUser | null) => user?.role === "Admin";
