"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useQueryClient } from "@tanstack/react-query";
import { Check, ChevronsUpDown, ExternalLink, LayoutDashboard, LogOut, Package, ShieldCheck, Store, Tags } from "lucide-react";
import { useMerchantAuthStore } from "@/stores/auth";
import { useTenantStore } from "@/stores/tenant";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

const NAV = [
  { href: "/admin/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/admin/products", label: "Products", icon: Package },
  { href: "/admin/categories", label: "Categories", icon: Tags },
  { href: "/admin/roles", label: "Roles & Permissions", icon: ShieldCheck },
] as const;

function StoreSwitcher() {
  const router = useRouter();
  const active = useTenantStore((s) => s.activeTenant);
  const stores = useTenantStore((s) => s.availableStores);
  const setActive = useTenantStore((s) => s.setActiveTenant);

  return (
    <DropdownMenu>
      <DropdownMenuTrigger render={<Button variant="outline" className="max-w-56 justify-between" />}>
        <Store />
        <span className="truncate">{active?.storeName ?? "Select a store"}</span>
        <ChevronsUpDown className="opacity-50" />
      </DropdownMenuTrigger>
      <DropdownMenuContent align="start" className="w-64">
        <DropdownMenuGroup>
          <DropdownMenuLabel>Switch store</DropdownMenuLabel>
          {stores.map((s) => (
            <DropdownMenuItem
              key={s.tenantId}
              onClick={() => {
                setActive(s);
                router.push("/admin/dashboard");
              }}
            >
              <span className="flex-1 truncate">{s.storeName}</span>
              {s.tenantId === active?.tenantId && <Check />}
            </DropdownMenuItem>
          ))}
        </DropdownMenuGroup>
        <DropdownMenuSeparator />
        {active && (
          <DropdownMenuItem render={<Link href={`/store/${active.tenantId}`} target="_blank" rel="noreferrer" />}>
            <ExternalLink /> Visit storefront
          </DropdownMenuItem>
        )}
        <DropdownMenuItem render={<Link href="/admin/stores" />}>
          <Store /> All stores
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

export function AdminShell({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const queryClient = useQueryClient();
  const user = useMerchantAuthStore((s) => s.user);
  const logout = useMerchantAuthStore((s) => s.logout);
  const resetTenant = useTenantStore((s) => s.reset);
  const hasStore = useTenantStore((s) => !!s.activeTenant);

  function signOut() {
    logout();
    resetTenant();
    queryClient.clear();
    router.replace("/admin/login");
  }

  return (
    <div className="flex min-h-screen flex-col">
      <header className="sticky top-0 z-30 border-b bg-background/90 backdrop-blur">
        <div className="flex h-14 items-center gap-3 px-4">
          <Link href="/admin/stores" className="font-semibold tracking-tight">
            BYV <span className="text-primary">Admin</span>
          </Link>
          <StoreSwitcher />
          <div className="ml-auto">
            <DropdownMenu>
              <DropdownMenuTrigger render={<Button variant="ghost" size="sm" />}>
                <span className="max-w-32 truncate">{user?.fullName}</span>
                <ChevronsUpDown className="opacity-50" />
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-56">
                <DropdownMenuGroup>
                  <DropdownMenuLabel className="truncate">{user?.email}</DropdownMenuLabel>
                </DropdownMenuGroup>
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={signOut}>
                  <LogOut /> Sign out
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>

        {hasStore && (
          <nav aria-label="Admin sections" className="flex gap-1 overflow-x-auto border-t px-3 md:hidden">
            {NAV.map(({ href, label, icon: Icon }) => (
              <Link
                key={href}
                href={href}
                className={cn(
                  "flex shrink-0 items-center gap-1.5 border-b-2 px-3 py-2 text-sm",
                  pathname.startsWith(href) ? "border-primary font-medium" : "border-transparent text-muted-foreground",
                )}
              >
                <Icon className="size-4" /> {label}
              </Link>
            ))}
          </nav>
        )}
      </header>

      <div className="flex flex-1">
        {hasStore && (
          <aside className="hidden w-56 shrink-0 border-r md:block">
            <nav aria-label="Admin sections" className="sticky top-14 space-y-1 p-3">
              {NAV.map(({ href, label, icon: Icon }) => (
                <Link
                  key={href}
                  href={href}
                  className={cn(
                    "flex items-center gap-2 rounded-lg px-3 py-2 text-sm transition-colors",
                    pathname.startsWith(href) ? "bg-primary/10 font-medium text-primary" : "text-muted-foreground hover:bg-muted",
                  )}
                >
                  <Icon className="size-4" /> {label}
                </Link>
              ))}
            </nav>
          </aside>
        )}
        <main className="min-w-0 flex-1 p-4 sm:p-6">{children}</main>
      </div>
    </div>
  );
}
