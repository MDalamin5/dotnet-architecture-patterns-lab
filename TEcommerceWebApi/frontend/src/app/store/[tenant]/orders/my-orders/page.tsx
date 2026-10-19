"use client";

import Link from "next/link";
import { ReceiptText } from "lucide-react";
import { useMyOrders } from "@/hooks/use-orders";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorAlert } from "@/components/common/error-alert";
import { OrderCard } from "@/components/storefront/order-card";
import { RequireLogin } from "@/components/storefront/require-login";
import { useStorefront } from "@/components/storefront/storefront-context";
import { buttonVariants } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";

function OrderList() {
  const { base } = useStorefront();
  const orders = useMyOrders();

  if (orders.isPending) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 3 }, (_, i) => (
          <Skeleton key={i} className="h-20 rounded-xl" />
        ))}
      </div>
    );
  }
  if (orders.isError) return <ErrorAlert error={orders.error} title="Couldn't load your orders" />;

  if (orders.data.length === 0) {
    return (
      <EmptyState
        icon={ReceiptText}
        title="No orders yet"
        description="When you place an order it will show up here."
        action={
          <Link href={base} className={buttonVariants()}>
            Start shopping
          </Link>
        }
      />
    );
  }

  const sorted = [...orders.data].sort((a, b) => +new Date(b.orderDate) - +new Date(a.orderDate));
  return (
    <div className="space-y-3">
      {sorted.map((o) => (
        <OrderCard key={o.orderId} order={o} />
      ))}
    </div>
  );
}

export default function MyOrdersPage() {
  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">My orders</h1>
      <RequireLogin>
        <OrderList />
      </RequireLogin>
    </div>
  );
}
