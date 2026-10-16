import { useAuthStore, useMerchantAuthStore } from "./auth";
import { useStorefrontTenantStore, useTenantStore } from "./tenant";

export const isAdminPath = (pathname: string) => pathname === "/admin" || pathname.startsWith("/admin/");

/** Which portal the browser is on right now (non-React callers such as the Axios interceptors). */
export function currentPortal(): "admin" | "store" {
  return typeof window !== "undefined" && isAdminPath(window.location.pathname) ? "admin" : "store";
}

export function currentSession() {
  const admin = currentPortal() === "admin";
  const auth = (admin ? useMerchantAuthStore : useAuthStore).getState();
  const tenantId = admin
    ? useTenantStore.getState().activeTenant?.tenantId
    : useStorefrontTenantStore.getState().tenant?.tenantId;
  return { portal: admin ? ("admin" as const) : ("store" as const), auth, tenantId };
}
