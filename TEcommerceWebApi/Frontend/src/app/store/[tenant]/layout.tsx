import { Suspense } from "react";
import { StorefrontShell } from "@/components/storefront/storefront-shell";
import { FullPageSpinner } from "@/components/common/spinner";

export default async function StorefrontLayout({ children, params }: LayoutProps<"/store/[tenant]">) {
  const { tenant } = await params;
  return (
    <Suspense fallback={<FullPageSpinner />}>
      <StorefrontShell tenantParam={decodeURIComponent(tenant)}>{children}</StorefrontShell>
    </Suspense>
  );
}
