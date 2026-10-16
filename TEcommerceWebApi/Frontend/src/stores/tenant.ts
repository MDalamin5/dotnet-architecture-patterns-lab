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
