"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { ExternalLink, Plus, Store } from "lucide-react";
import { useMyStores } from "@/hooks/use-admin";
import { env } from "@/lib/env";
import { formatDate } from "@/lib/format";
import { useTenantStore } from "@/stores/tenant";
import type { TenantReadDto } from "@/types/api";
import { CreateStoreDialog } from "@/components/admin/create-store-dialog";
import { PageHeader } from "@/components/admin/page-header";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorAlert } from "@/components/common/error-alert";
import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";

export default function StoresPage() {
  const router = useRouter();
  const stores = useMyStores();
  const setAvailableStores = useTenantStore((s) => s.setAvailableStores);
  const activeId = useTenantStore((s) => s.activeTenant?.tenantId);
  const setActiveTenant = useTenantStore((s) => s.setActiveTenant);
  const [creating, setCreating] = useState(false);

  // Keep the store switcher in the header in sync with the server.
  useEffect(() => {
    if (stores.data) {
      setAvailableStores(
        stores.data.map(({ tenantId, storeName, subdomain, customDomain }) => ({ tenantId, storeName, subdomain, customDomain })),
      );
    }
  }, [stores.data, setAvailableStores]);

  function manage(store: TenantReadDto) {
    setActiveTenant({
      tenantId: store.tenantId,
      storeName: store.storeName,
      subdomain: store.subdomain,
      customDomain: store.customDomain,
    });
    router.push("/admin/dashboard");
  }

  return (
    <div className="mx-auto max-w-5xl">
      <PageHeader
        title="Your stores"
        description="Pick a store to manage, or launch a new one."
        actions={
          <Button onClick={() => setCreating(true)}>
            <Plus /> Create new store
          </Button>
        }
      />

      <ErrorAlert error={stores.error} title="Couldn't load your stores" className="mb-4" />

      {stores.isPending ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 3 }, (_, i) => (
            <Skeleton key={i} className="h-44 rounded-xl" />
          ))}
        </div>
      ) : stores.data && stores.data.length === 0 ? (
        <EmptyState
          icon={Store}
          title="No stores yet"
          description="Create your first store to start selling."
          action={<Button onClick={() => setCreating(true)}>Create new store</Button>}
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {stores.data?.map((store) => (
            <Card key={store.tenantId} className={cn(store.tenantId === activeId && "ring-2 ring-primary")}>
              <CardHeader>
                <div className="flex items-start justify-between gap-2">
                  <CardTitle className="text-base">{store.storeName}</CardTitle>
                  <Badge
                    className={
                      store.isActive
                        ? "bg-green-100 text-green-800 dark:bg-green-500/20 dark:text-green-300"
                        : "bg-muted text-muted-foreground"
                    }
                  >
                    {store.isActive ? "Active" : "Inactive"}
                  </Badge>
                </div>
              </CardHeader>
              <CardContent className="space-y-1 text-sm">
                <p className="text-muted-foreground">
                  {store.subdomain}.{env.rootDomain}
                </p>
                {store.customDomain && <p className="text-muted-foreground">{store.customDomain}</p>}
                <p className="text-xs text-muted-foreground">Created {formatDate(store.createdAt)}</p>
              </CardContent>
              <CardFooter className="gap-2">
                <Button className="flex-1" onClick={() => manage(store)}>
                  Manage store
                </Button>
                <Link
                  href={`/store/${store.tenantId}`}
                  target="_blank"
                  rel="noreferrer"
                  aria-label={`Visit ${store.storeName} storefront`}
                  className={buttonVariants({ variant: "outline", size: "icon" })}
                >
                  <ExternalLink />
                </Link>
              </CardFooter>
            </Card>
          ))}
        </div>
      )}

      <CreateStoreDialog open={creating} onOpenChange={setCreating} />
    </div>
  );
}
