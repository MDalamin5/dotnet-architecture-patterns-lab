"use client";

import { useEffect, useMemo } from "react";
import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { StoreIcon } from "lucide-react";
import { tenantsApi } from "@/lib/api";
import { isGuid } from "@/lib/format";
import { useAuthStore } from "@/stores/auth";
import { useCartStore } from "@/stores/cart";
import { useHydrated } from "@/stores/hydration";
import { useStorefrontTenantStore, useTenantStore, type ActiveTenant } from "@/stores/tenant";
import { FullPageSpinner } from "@/components/common/spinner";
import { EmptyState } from "@/components/common/empty-state";
import { buttonVariants } from "@/components/ui/button";
import { StorefrontContext } from "./storefront-context";
import { StorefrontHeader } from "./storefront-header";
import { CartDrawer } from "./cart-drawer";
import { AuthDialog } from "./auth-dialog";
import { OrderPlacedDialog } from "./order-placed-dialog";

/**
 * Resolves which tenant this storefront belongs to, pins it as the storefront tenant (so the Axios
 * interceptor sends X-Tenant-Id) and only renders the storefront once that is in place.
 *
 * `tenantParam` is either a tenant GUID (local dev / admin "Visit storefront" links) or a
 * subdomain handed over by proxy.ts, which needs a lookup endpoint (see tenantsApi.bySubdomain).
 */
export function StorefrontShell({ tenantParam, children }: { tenantParam: string; children: React.ReactNode }) {
  const hydrated = useHydrated();
  const byId = isGuid(tenantParam);
  // Merchants who opened "Visit storefront" already know the store name; guests fall back to a generic one.
  const availableStores = useTenantStore((s) => s.availableStores);
  const activeTenant = useStorefrontTenantStore((s) => s.tenant);

  const lookup = useQuery({
    queryKey: ["tenant", "by-subdomain", tenantParam],
    queryFn: () => tenantsApi.bySubdomain(tenantParam),
    enabled: hydrated && !byId,
    retry: false,
    staleTime: 5 * 60_000,
  });

  const resolved = useMemo<ActiveTenant | null>(() => {
    if (!hydrated) return null;
    if (byId) {
      const tenantId = tenantParam.toLowerCase();
      const known = availableStores.find((s) => s.tenantId.toLowerCase() === tenantId);
      return { tenantId, storeName: known?.storeName ?? "Online Store", subdomain: known?.subdomain ?? "" };
    }
    if (lookup.data) {
      const { tenantId, storeName, subdomain, customDomain } = lookup.data;
      return { tenantId, storeName, subdomain, customDomain };
    }
    return null;
  }, [hydrated, byId, tenantParam, availableStores, lookup.data]);

  useEffect(() => {
    if (!resolved) return;

    // A cart and a customer session never leak between stores.
    useCartStore.getState().bindTenant(resolved.tenantId);
    const { user, logout } = useAuthStore.getState();
    if (user && user.role !== "Admin" && user.tenantId && user.tenantId.toLowerCase() !== resolved.tenantId) {
      logout();
    }

    const current = useStorefrontTenantStore.getState().tenant;
    if (current?.tenantId !== resolved.tenantId || current.storeName !== resolved.storeName) {
      useStorefrontTenantStore.getState().setTenant(resolved);
    }
  }, [resolved]);

  if (!hydrated || (!byId && lookup.isPending)) return <FullPageSpinner />;

  if (!byId && lookup.isError) {
    return (
      <main className="mx-auto flex w-full max-w-xl flex-1 items-center px-4">
        <div className="w-full">
          <EmptyState
            icon={StoreIcon}
            title="Store not found"
            description={`We couldn't find a store for "${tenantParam}". Check the address, or ask the merchant for their store link.`}
            action={
              <Link href="/" className={buttonVariants({ variant: "outline" })}>
                Back to home
              </Link>
            }
          />
        </div>
      </main>
    );
  }

  if (!resolved || activeTenant?.tenantId !== resolved.tenantId) return <FullPageSpinner />;

  return (
    <StorefrontContext.Provider value={{ base: `/store/${tenantParam}`, tenant: activeTenant }}>
      <StorefrontHeader />
      <main className="mx-auto w-full max-w-7xl flex-1 px-4 py-6 sm:px-6">{children}</main>
      <footer className="border-t py-6 text-center text-sm text-muted-foreground">
        © {new Date().getFullYear()} {activeTenant.storeName}
      </footer>
      <CartDrawer />
      <AuthDialog />
      <OrderPlacedDialog />
    </StorefrontContext.Provider>
  );
}
