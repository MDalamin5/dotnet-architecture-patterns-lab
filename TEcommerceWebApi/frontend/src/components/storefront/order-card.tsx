"use client";

import { useState } from "react";
import { ChevronDown } from "lucide-react";
import { formatCurrency, formatDate } from "@/lib/format";
import { cn } from "@/lib/utils";
import type { OrderReadDto } from "@/types/api";
import { OrderStatusBadge } from "@/components/common/status-badges";
import { Card } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

export const shortOrderId = (id: string) => id.slice(0, 8).toUpperCase();

export function OrderCard({ order, defaultOpen = false }: { order: OrderReadDto; defaultOpen?: boolean }) {
  const [open, setOpen] = useState(defaultOpen);
  const panelId = `order-${order.orderId}`;

  return (
    <Card className="gap-0 py-0">
      <button
        type="button"
        aria-expanded={open}
        aria-controls={panelId}
        onClick={() => setOpen((o) => !o)}
        className="flex w-full flex-wrap items-center gap-x-6 gap-y-2 p-4 text-left"
      >
        <div className="min-w-32">
          <p className="text-xs text-muted-foreground">Order</p>
          <p className="font-mono text-sm font-medium">#{shortOrderId(order.orderId)}</p>
        </div>
        <div className="min-w-28">
          <p className="text-xs text-muted-foreground">Date</p>
          <p className="text-sm">{formatDate(order.orderDate)}</p>
        </div>
        <div className="min-w-24">
          <p className="text-xs text-muted-foreground">Total</p>
          <p className="text-sm font-medium tabular-nums">{formatCurrency(order.totalAmount)}</p>
        </div>
        <OrderStatusBadge status={order.status} />
        <ChevronDown className={cn("ml-auto size-4 text-muted-foreground transition-transform", open && "rotate-180")} />
      </button>

      {open && (
        <div id={panelId} className="border-t p-4">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Product</TableHead>
                <TableHead className="text-right">Unit price</TableHead>
                <TableHead className="text-right">Qty</TableHead>
                <TableHead className="text-right">Subtotal</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {order.items.map((item) => (
                <TableRow key={item.orderItemId}>
                  <TableCell>
                    <p className="font-medium">{item.productName}</p>
                    <p className="text-xs text-muted-foreground">{item.categoryName}</p>
                  </TableCell>
                  <TableCell className="text-right tabular-nums">{formatCurrency(item.unitPrice)}</TableCell>
                  <TableCell className="text-right tabular-nums">{item.quantity}</TableCell>
                  <TableCell className="text-right tabular-nums">{formatCurrency(item.subTotal)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
          <p className="mt-3 text-xs text-muted-foreground">Invoice ID: {order.orderId}</p>
        </div>
      )}
    </Card>
  );
}
