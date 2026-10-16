import { useEffect, useState } from "react";
import { create } from "zustand";
import { useAuthStore } from "./auth";
import { useCartStore } from "./cart";
import { useTenantStore } from "./tenant";

/**
 * Persisted stores use `skipHydration`, so the first client render matches the server HTML.
 * <Providers /> calls `rehydrateStores()` once after mount; anything that depends on persisted
 * state (cart badge, auth guards) should wait for `useHydrated()`.
 */
const useHydrationFlag = create<{ hydrated: boolean }>(() => ({ hydrated: false }));

let started = false;

export async function rehydrateStores() {
  if (started) return;
  started = true;
  await Promise.all([
    useAuthStore.persist.rehydrate(),
    useTenantStore.persist.rehydrate(),
    useCartStore.persist.rehydrate(),
  ]);
  useHydrationFlag.setState({ hydrated: true });
}

export const useHydrated = () => useHydrationFlag((s) => s.hydrated);

/** Re-render-safe "have we mounted on the client" flag for non-store client-only values. */
export function useMounted() {
  const [mounted, setMounted] = useState(false);
  useEffect(() => setMounted(true), []);
  return mounted;
}
