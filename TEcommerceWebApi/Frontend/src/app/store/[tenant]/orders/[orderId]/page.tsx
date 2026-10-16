"use client";

import { use } from "react";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { useOrder } from "@/hooks/use-orders";
import { ErrorAlert } from "@/components/common/error-alert";
import { OrderCard } from "@/components/storefront/order-card";
import { RequireLogin } from "@/components/storefront/require-login";
import { useStorefront } from "@/components/storefront/storefront-context";
import { Skeleton } from "@/components/ui/skeleton";

function OrderDetail({ orderId }: { orderId: string }) {
  const order = useOrder(orderId);
  if (order.isPending) return <Skeleton className="h-40 rounded-xl" />;
  if (order.isError) return <ErrorAlert error={order.error} title="Couldn't load this order" />;
  return <OrderCard order={order.data} defaultOpen />;
}

export default function OrderDetailPage({ params }: PageProps<"/store/[tenant]/orders/[orderId]">) {
  const { orderId } = use(params);
  const { base } = useStorefront();

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <Link href={`${base}/orders/my-orders`} className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground">
        <ArrowLeft className="size-4" /> All orders
      </Link>
      <h1 className="text-2xl font-semibold tracking-tight">Order details</h1>
      <RequireLogin>
        <OrderDetail orderId={orderId} />
      </RequireLogin>
    </div>
  );
}
