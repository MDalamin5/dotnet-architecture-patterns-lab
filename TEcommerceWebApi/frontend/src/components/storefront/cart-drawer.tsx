"use client";

import { Minus, Plus, ShoppingBag, Trash2 } from "lucide-react";
import { useCheckout } from "@/hooks/use-orders";
import { formatCurrency } from "@/lib/format";
import { useAuthStore } from "@/stores/auth";
import { MAX_LINE_QUANTITY, useCartStore } from "@/stores/cart";
import { useStorefrontUi } from "@/stores/ui";
import { ErrorAlert } from "@/components/common/error-alert";
import { ProductImage } from "@/components/common/product-image";
import { Spinner } from "@/components/common/spinner";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { Sheet, SheetContent, SheetDescription, SheetFooter, SheetHeader, SheetTitle } from "@/components/ui/sheet";
import { cn } from "@/lib/utils";
import { getErrorMessages } from "@/lib/errors";

export function CartDrawer() {
  const { cartOpen, setCartOpen, openAuth, setPlacedOrder } = useStorefrontUi();
  const token = useAuthStore((s) => s.token);
  const { items, updateQuantity, removeItem, clearCart } = useCartStore();
  const total = useCartStore((s) => s.items.reduce((sum, i) => sum + i.price * i.quantity, 0));
  const itemCount = useCartStore((s) => s.items.reduce((n, i) => n + i.quantity, 0));
  const checkout = useCheckout();

  // The backend's stock error reads "Insufficient stock for '<name>'. Available: n, Requested: m."
  const failedNames = checkout.isError
    ? getErrorMessages(checkout.error).flatMap((m) => [...m.matchAll(/for '([^']+)'/g)].map((x) => x[1]))
    : [];

  function handleCheckout() {
    if (!token) {
      openAuth("login");
      return;
    }
    checkout.mutate(
      { items: items.map(({ productId, quantity }) => ({ productId, quantity })) },
      {
        onSuccess: (order) => {
          clearCart();
          setCartOpen(false);
          setPlacedOrder(order);
        },
      },
    );
  }

  return (
    <Sheet
      open={cartOpen}
      onOpenChange={(open) => {
        setCartOpen(open);
        if (!open) checkout.reset();
      }}
    >
      <SheetContent className="w-full sm:max-w-md">
        <SheetHeader className="border-b">
          <SheetTitle>Your cart ({itemCount})</SheetTitle>
          <SheetDescription>Review your items before checking out.</SheetDescription>
        </SheetHeader>

        {items.length === 0 ? (
          <div className="flex flex-1 flex-col items-center justify-center gap-3 p-6 text-center text-muted-foreground">
            <ShoppingBag className="size-10" />
            <p>Your cart is empty.</p>
            <Button variant="outline" onClick={() => setCartOpen(false)}>
              Continue shopping
            </Button>
          </div>
        ) : (
          <>
            <ul className="flex-1 space-y-4 overflow-y-auto px-4">
              {items.map((item) => {
                const flagged = failedNames.includes(item.name);
                return (
                  <li
                    key={item.productId}
                    className={cn("flex gap-3 rounded-lg p-2", flagged && "bg-destructive/10 ring-1 ring-destructive/40")}
                  >
                    <ProductImage src={item.imageUrl} alt={item.name} className="size-16 shrink-0 rounded-md" />
                    <div className="min-w-0 flex-1 space-y-2">
                      <div className="flex items-start justify-between gap-2">
                        <p className="line-clamp-2 text-sm font-medium">{item.name}</p>
                        <Button
                          variant="ghost"
                          size="icon-xs"
                          onClick={() => removeItem(item.productId)}
                          aria-label={`Remove ${item.name}`}
                        >
                          <Trash2 />
                        </Button>
                      </div>
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-1">
                          <Button
                            variant="outline"
                            size="icon-xs"
                            onClick={() => updateQuantity(item.productId, item.quantity - 1)}
                            aria-label="Decrease quantity"
                          >
                            <Minus />
                          </Button>
                          <span className="w-8 text-center text-sm tabular-nums">{item.quantity}</span>
                          <Button
                            variant="outline"
                            size="icon-xs"
                            disabled={item.quantity >= Math.min(item.stockQuantity, MAX_LINE_QUANTITY)}
                            onClick={() => updateQuantity(item.productId, item.quantity + 1)}
                            aria-label="Increase quantity"
                          >
                            <Plus />
                          </Button>
                        </div>
                        <p className="text-sm font-medium tabular-nums">{formatCurrency(item.price * item.quantity)}</p>
                      </div>
                      <p className="text-xs text-muted-foreground">{formatCurrency(item.price)} each</p>
                    </div>
                  </li>
                );
              })}
            </ul>

            <SheetFooter className="border-t">
              <ErrorAlert error={checkout.error} title="Couldn't place your order" />
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Subtotal</span>
                <span className="tabular-nums">{formatCurrency(total)}</span>
              </div>
              <Separator />
              <div className="flex justify-between font-semibold">
                <span>Total</span>
                <span className="tabular-nums">{formatCurrency(total)}</span>
              </div>
              <Button size="lg" className="h-10 w-full" onClick={handleCheckout} disabled={checkout.isPending}>
                {checkout.isPending ? (
                  <>
                    <Spinner /> Placing order…
                  </>
                ) : token ? (
                  "Checkout"
                ) : (
                  "Sign in to checkout"
                )}
              </Button>
              <Button variant="ghost" size="sm" onClick={clearCart} disabled={checkout.isPending}>
                Clear cart
              </Button>
            </SheetFooter>
          </>
        )}
      </SheetContent>
    </Sheet>
  );
}
