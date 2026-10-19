"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useMerchantAuthStore } from "@/stores/auth";
import { useHydrated } from "@/stores/hydration";
import { useTenantStore } from "@/stores/tenant";
import { FullPageSpinner } from "@/components/common/spinner";

/** Only signed-in store owners (role Admin) may see the portal. */
export function AdminGuard({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const hydrated = useHydrated();
  const token = useMerchantAuthStore((s) => s.token);
  const role = useMerchantAuthStore((s) => s.user?.role);
  const allowed = !!token && role === "Admin";

  useEffect(() => {
    if (hydrated && !allowed) router.replace("/admin/login");
  }, [hydrated, allowed, router]);

  if (!hydrated || !allowed) return <FullPageSpinner />;
  return <>{children}</>;
}

/** Pages that operate on one store need an active tenant (X-Tenant-Id); otherwise pick one first. */
export function RequireTenant({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const active = useTenantStore((s) => s.activeTenant);

  useEffect(() => {
    if (!active) router.replace("/admin/stores");
  }, [active, router]);

  if (!active) return <FullPageSpinner />;
  return <>{children}</>;
}
