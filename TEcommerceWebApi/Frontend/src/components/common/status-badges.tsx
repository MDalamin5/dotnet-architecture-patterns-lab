import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

const ORDER_STATUS_STYLES: Record<string, string> = {
  Pending: "bg-yellow-100 text-yellow-800 dark:bg-yellow-500/20 dark:text-yellow-300",
  Processing: "bg-blue-100 text-blue-800 dark:bg-blue-500/20 dark:text-blue-300",
  Completed: "bg-green-100 text-green-800 dark:bg-green-500/20 dark:text-green-300",
  Cancelled: "bg-red-100 text-red-800 dark:bg-red-500/20 dark:text-red-300",
};

export function OrderStatusBadge({ status }: { status: string }) {
  return <Badge className={cn(ORDER_STATUS_STYLES[status] ?? "bg-muted text-muted-foreground")}>{status}</Badge>;
}

/** Thresholds from the spec: >5 in stock, 1-5 low, 0 out. */
export const LOW_STOCK_THRESHOLD = 5;

export function StockBadge({ quantity }: { quantity: number }) {
  if (quantity <= 0) {
    return <Badge variant="destructive">Out of stock</Badge>;
  }
  if (quantity <= LOW_STOCK_THRESHOLD) {
    return (
      <Badge className="bg-orange-100 text-orange-800 dark:bg-orange-500/20 dark:text-orange-300">
        Low stock · {quantity} left
      </Badge>
    );
  }
  return <Badge className="bg-green-100 text-green-800 dark:bg-green-500/20 dark:text-green-300">In stock</Badge>;
}
