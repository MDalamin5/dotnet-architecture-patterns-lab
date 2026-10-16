"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useQueryClient } from "@tanstack/react-query";
import { LogOut, Package, Search, ShoppingCart, User } from "lucide-react";
import { useDebounce } from "@/hooks/use-debounce";
import { useAuthStore } from "@/stores/auth";
import { useCartStore } from "@/stores/cart";
import { useStorefrontUi } from "@/stores/ui";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useStorefront } from "./storefront-context";

function SearchBox() {
  const { base } = useStorefront();
  const router = useRouter();
  const params = useSearchParams();
  const urlQuery = params.get("q") ?? "";
  const urlCategory = params.get("category");

  const [value, setValue] = useState(urlQuery);
  const debounced = useDebounce(value.trim(), 300);

  // Picking a category pill clears the search box (adjusting state during render, per React docs).
  const [seenCategory, setSeenCategory] = useState(urlCategory);
  if (urlCategory !== seenCategory) {
    setSeenCategory(urlCategory);
    if (urlCategory) setValue("");
  }

  // Search (300ms debounce) drives ?q=; it replaces any category filter because the API has a
  // single searchValue that matches product *or* category name. Only push once typing has settled.
  useEffect(() => {
    if (value.trim() !== debounced || debounced === urlQuery) return;
    router.replace(debounced ? `${base}?q=${encodeURIComponent(debounced)}` : base);
  }, [value, debounced, urlQuery, base, router]);

  return (
    <div className="relative w-full">
      <Search className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground" />
      <Input
        type="search"
        value={value}
        onChange={(e) => setValue(e.target.value)}
        placeholder="Search products…"
        aria-label="Search products"
        className="h-9 pl-8"
      />
    </div>
  );
}

export function StorefrontHeader() {
  const { base, tenant } = useStorefront();
  const queryClient = useQueryClient();
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const totalItems = useCartStore((s) => s.items.reduce((n, i) => n + i.quantity, 0));
  const { setCartOpen, openAuth } = useStorefrontUi();

  return (
    <header className="sticky top-0 z-30 border-b bg-background/90 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-7xl items-center gap-3 px-4 sm:gap-6 sm:px-6">
        <Link href={base} className="shrink-0 text-lg font-semibold tracking-tight">
          {tenant.storeName}
        </Link>

        <div className="mx-auto w-full max-w-md">
          <SearchBox />
        </div>

        <div className="flex shrink-0 items-center gap-1">
          {user ? (
            <DropdownMenu>
              <DropdownMenuTrigger render={<Button variant="ghost" size="sm" aria-label="Account menu" />}>
                <User />
                <span className="hidden max-w-28 truncate sm:inline">{user.fullName}</span>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-52">
                <DropdownMenuGroup>
                  <DropdownMenuLabel className="truncate">{user.email}</DropdownMenuLabel>
                </DropdownMenuGroup>
                <DropdownMenuSeparator />
                <DropdownMenuItem render={<Link href={`${base}/orders/my-orders`} />}>
                  <Package /> My Orders
                </DropdownMenuItem>
                <DropdownMenuItem
                  onClick={() => {
                    logout();
                    queryClient.removeQueries({ queryKey: ["orders"] });
                  }}
                >
                  <LogOut /> Logout
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          ) : (
            <Button variant="ghost" size="sm" onClick={() => openAuth("login")}>
              <User /> <span className="hidden sm:inline">Login</span>
            </Button>
          )}

          <Button variant="outline" size="icon" className="relative" onClick={() => setCartOpen(true)} aria-label={`Open cart, ${totalItems} items`}>
            <ShoppingCart />
            {totalItems > 0 && (
              <span className="absolute -top-1.5 -right-1.5 flex min-w-4 items-center justify-center rounded-full bg-primary px-1 text-[10px] leading-4 font-semibold text-primary-foreground">
                {totalItems}
              </span>
            )}
          </Button>
        </div>
      </div>
    </header>
  );
}
