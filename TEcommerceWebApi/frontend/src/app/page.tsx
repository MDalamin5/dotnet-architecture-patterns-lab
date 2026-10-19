import Link from "next/link";
import { ArrowRight, Store, LayoutDashboard } from "lucide-react";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export default function PlatformHome() {
  return (
    <main className="mx-auto flex w-full max-w-4xl flex-1 flex-col justify-center gap-10 px-4 py-20">
      <div className="space-y-4">
        <p className="text-sm font-medium text-primary">BYV Store · multi-tenant commerce</p>
        <h1 className="text-4xl font-semibold tracking-tight sm:text-5xl">
          One platform. Every merchant gets their own storefront.
        </h1>
        <p className="max-w-2xl text-muted-foreground">
          Launch branded stores, manage inventory and roles, and watch sales — each tenant is fully isolated.
        </p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-3 rounded-2xl border p-6">
          <LayoutDashboard className="size-6 text-primary" />
          <h2 className="font-medium">Merchant portal</h2>
          <p className="text-sm text-muted-foreground">Sign in to manage your stores, products, categories and roles.</p>
          <Link href="/admin/login" className={cn(buttonVariants(), "w-fit")}>
            Open admin <ArrowRight />
          </Link>
        </div>
        <div className="space-y-3 rounded-2xl border p-6">
          <Store className="size-6 text-primary" />
          <h2 className="font-medium">Shop a store</h2>
          <p className="text-sm text-muted-foreground">
            Storefronts live at <code className="rounded bg-muted px-1">/store/&lt;tenantId&gt;</code> locally and at{" "}
            <code className="rounded bg-muted px-1">&lt;subdomain&gt;.byvstore.com</code> in production. Merchants get the
            link from the store switcher.
          </p>
        </div>
      </div>
    </main>
  );
}
