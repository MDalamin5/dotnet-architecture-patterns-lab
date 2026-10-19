"use client";

import { AlertTriangle, DollarSign, Package, Users, type LucideIcon } from "lucide-react";
import { useSalesOverview, useTopSellingProducts } from "@/hooks/use-admin";
import { formatCurrency, formatNumber } from "@/lib/format";
import { cn } from "@/lib/utils";
import { PageHeader } from "@/components/admin/page-header";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorAlert } from "@/components/common/error-alert";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

function Kpi({
  label,
  value,
  icon: Icon,
  loading,
  tone,
  hint,
}: {
  label: string;
  value?: string;
  icon: LucideIcon;
  loading: boolean;
  tone?: "warn";
  hint?: string;
}) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0">
        <CardDescription>{label}</CardDescription>
        <Icon className={cn("size-4 text-muted-foreground", tone === "warn" && "text-orange-500")} />
      </CardHeader>
      <CardContent>
        {loading ? <Skeleton className="h-8 w-24" /> : <p className="text-2xl font-semibold tabular-nums">{value}</p>}
        {hint && <p className="mt-1 text-xs text-muted-foreground">{hint}</p>}
      </CardContent>
    </Card>
  );
}

export default function DashboardPage() {
  const overview = useSalesOverview();
  const top = useTopSellingProducts(5);
  const data = overview.data;

  return (
    <div className="mx-auto max-w-6xl">
      <PageHeader title="Dashboard" description="Sales overview for the active store." />

      <ErrorAlert error={overview.error} title="Couldn't load sales overview" className="mb-4" />

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <Kpi label="Total revenue" icon={DollarSign} loading={overview.isPending} value={data && formatCurrency(data.totalRevenue)} />
        <Kpi label="Total orders" icon={Package} loading={overview.isPending} value={data && formatNumber(data.totalOrdersCount)} />
        <Kpi label="Total customers" icon={Users} loading={overview.isPending} value={data && formatNumber(data.totalCustomersCount)} />
        <Kpi
          label="Low stock alerts"
          icon={AlertTriangle}
          tone={data && data.lowStockProductsCount > 0 ? "warn" : undefined}
          loading={overview.isPending}
          value={data && formatNumber(data.lowStockProductsCount)}
          hint={data ? `${formatNumber(data.totalActiveProducts)} active products` : undefined}
        />
      </div>

      <Card className="mt-6">
        <CardHeader>
          <CardTitle>Top selling products</CardTitle>
          <CardDescription>Ranked by units sold.</CardDescription>
        </CardHeader>
        <CardContent>
          <ErrorAlert error={top.error} title="Couldn't load top products" />
          {top.isPending ? (
            <div className="space-y-2">
              {Array.from({ length: 5 }, (_, i) => (
                <Skeleton key={i} className="h-10" />
              ))}
            </div>
          ) : top.data && top.data.length === 0 ? (
            <EmptyState icon={Package} title="No sales yet" description="Top sellers appear once customers place orders." />
          ) : (
            top.data && (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-10">#</TableHead>
                    <TableHead>Product</TableHead>
                    <TableHead className="text-right">Units sold</TableHead>
                    <TableHead className="text-right">Revenue</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {top.data.map((p, i) => (
                    <TableRow key={p.productId}>
                      <TableCell className="text-muted-foreground">{i + 1}</TableCell>
                      <TableCell>
                        <p className="font-medium">{p.productName}</p>
                        <p className="text-xs text-muted-foreground">{p.categoryName}</p>
                      </TableCell>
                      <TableCell className="text-right tabular-nums">{formatNumber(p.totalUnitsSold)}</TableCell>
                      <TableCell className="text-right tabular-nums">{formatCurrency(p.totalRevenue)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )
          )}
        </CardContent>
      </Card>
    </div>
  );
}
