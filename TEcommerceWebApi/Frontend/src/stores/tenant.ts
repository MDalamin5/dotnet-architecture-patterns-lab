import { create } from "zustand";
import { persist } from "zustand/middleware";

export interface ActiveTenant {
  tenantId: string;
  storeName: string;
  subdomain: string;
  customDomain?: string | null;
}

interface TenantState {
  activeTenant: ActiveTenant | null;
  availableStores: ActiveTenant[];
  setActiveTenant: (tenant: ActiveTenant | null) => void;
  setAvailableStores: (stores: ActiveTenant[]) => void;
  reset: () => void;
}

export const useTenantStore = create<TenantState>()(
  persist(
    (set) => ({
      activeTenant: null,
      availableStores: [],
      setActiveTenant: (activeTenant) => set({ activeTenant }),
      setAvailableStores: (availableStores) => set({ availableStores }),
      reset: () => set({ activeTenant: null, availableStores: [] }),
    }),
    { name: "byv-tenant", skipHydration: true },
  ),
);

/**
 * The store a *customer* is currently browsing (/store/<tenant>). Kept apart from `useTenantStore`
 * so visiting a storefront never changes which store the merchant is managing.
 */
interface StorefrontTenantState {
  tenant: ActiveTenant | null;
  setTenant: (tenant: ActiveTenant | null) => void;
}

export const useStorefrontTenantStore = create<StorefrontTenantState>()(
  persist(
    (set) => ({ tenant: null, setTenant: (tenant) => set({ tenant }) }),
    { name: "byv-storefront-tenant", skipHydration: true },
  ),
);
