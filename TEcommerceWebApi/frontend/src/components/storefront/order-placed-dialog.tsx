"use client";

import { useRouter } from "next/navigation";
import { CheckCircle2 } from "lucide-react";
import { formatCurrency } from "@/lib/format";
import { useStorefrontUi } from "@/stores/ui";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { useStorefront } from "./storefront-context";

export function OrderPlacedDialog() {
  const router = useRouter();
  const { base } = useStorefront();
  const { placedOrder, setPlacedOrder } = useStorefrontUi();

  function close() {
    if (!placedOrder) return;
    const id = placedOrder.orderId;
    setPlacedOrder(null);
    router.push(`${base}/orders/${id}`);
  }

  return (
    <Dialog open={!!placedOrder} onOpenChange={(open) => !open && close()}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader className="items-center text-center">
          <CheckCircle2 className="size-12 text-green-600" />
          <DialogTitle>Order placed!</DialogTitle>
          <DialogDescription>Thank you for your purchase. Keep your invoice ID for reference.</DialogDescription>
        </DialogHeader>
        {placedOrder && (
          <dl className="space-y-1 rounded-lg bg-muted p-3 text-sm">
            <div className="flex justify-between gap-4">
              <dt className="text-muted-foreground">Invoice ID</dt>
              <dd className="font-mono text-xs break-all">{placedOrder.orderId}</dd>
            </div>
            <div className="flex justify-between">
              <dt className="text-muted-foreground">Total</dt>
              <dd className="font-medium tabular-nums">{formatCurrency(placedOrder.totalAmount)}</dd>
            </div>
          </dl>
        )}
        <DialogFooter>
          <Button onClick={close} className="w-full">
            View order
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
