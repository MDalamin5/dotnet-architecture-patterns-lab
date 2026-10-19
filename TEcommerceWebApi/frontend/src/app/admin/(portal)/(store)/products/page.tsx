"use client";

import { useState } from "react";
import { Pencil, Plus, Search, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { useDebounce } from "@/hooks/use-debounce";
import { useProductMutations, useProducts } from "@/hooks/use-catalog";
import { getErrorMessage } from "@/lib/errors";
import { formatCurrency } from "@/lib/format";
import { useMerchantAuthStore } from "@/stores/auth";
import type { ProductReadDto, SortOrder } from "@/types/api";
import { PageHeader } from "@/components/admin/page-header";
import { ProductFormDialog } from "@/components/admin/product-form-dialog";
import { ConfirmDialog } from "@/components/common/confirm-dialog";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorAlert } from "@/components/common/error-alert";
import { Pagination } from "@/components/common/pagination";
import { ProductImage } from "@/components/common/product-image";
import { StockBadge } from "@/components/common/status-badges";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Package } from "lucide-react";

const PAGE_SIZE = 6; // backend max

const SORT_OPTIONS: { value: SortOrder; label: string }[] = [
  { value: "CreatedAtDesc", label: "Newest first" },
  { value: "CreatedAtAsc", label: "Oldest first" },
  { value: "NameAsc", label: "Name A–Z" },
  { value: "NameDesc", label: "Name Z–A" },
];

export default function AdminProductsPage() {
  const canCreate = useMerchantAuthStore((s) => s.hasPermission("products.create"));
  const [search, setSearch] = useState("");
  const [sortOrder, setSortOrder] = useState<SortOrder>("CreatedAtDesc");
  const [page, setPage] = useState(1);
  const searchValue = useDebounce(search.trim(), 300);

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<ProductReadDto | null>(null);
  const [deleting, setDeleting] = useState<ProductReadDto | null>(null);

  const products = useProducts({ pageNumber: page, pageSize: PAGE_SIZE, searchValue: searchValue || undefined, sortOrder });
  const { remove } = useProductMutations();

  function openCreate() {
    setEditing(null);
    setFormOpen(true);
  }

  function confirmDelete() {
    if (!deleting) return;
    remove.mutate(deleting.productId, {
      onSuccess: () => {
        toast.success(`“${deleting.name}” deleted`);
        setDeleting(null);
        // Deleting the last row of a page would otherwise leave an empty page.
        if (products.data && products.data.items.length === 1 && page > 1) setPage(page - 1);
      },
      onError: (err) => toast.error(getErrorMessage(err, "Couldn't delete the product")),
    });
  }

  return (
    <div className="mx-auto max-w-6xl">
      <PageHeader
        title="Products"
        description="Manage inventory for the active store."
        actions={
          <Button onClick={openCreate} disabled={!canCreate} title={canCreate ? undefined : "You need the products.create permission"}>
            <Plus /> Add new product
          </Button>
        }
      />

      <div className="mb-4 flex flex-wrap gap-3">
        <div className="relative min-w-52 flex-1 sm:max-w-xs">
          <Search className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
            placeholder="Search products or categories…"
            aria-label="Search products"
            className="pl-8"
          />
        </div>
        <Select
          value={sortOrder}
          onValueChange={(v) => {
            setSortOrder(v as SortOrder);
            setPage(1);
          }}
          items={SORT_OPTIONS}
        >
          <SelectTrigger className="w-44" aria-label="Sort products">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {SORT_OPTIONS.map((o) => (
              <SelectItem key={o.value} value={o.value}>
                {o.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <ErrorAlert error={products.error} title="Couldn't load products" className="mb-4" />

      {products.isPending ? (
        <div className="space-y-2">
          {Array.from({ length: 5 }, (_, i) => (
            <Skeleton key={i} className="h-16" />
          ))}
        </div>
      ) : products.data && products.data.items.length === 0 ? (
        <EmptyState
          icon={Package}
          title={searchValue ? "No matching products" : "No products yet"}
          description={searchValue ? "Try a different search." : "Add your first product to start selling."}
          action={!searchValue && canCreate ? <Button onClick={openCreate}>Add new product</Button> : undefined}
        />
      ) : (
        products.data && (
          <Card className="overflow-hidden py-0">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-16">Image</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Category</TableHead>
                  <TableHead className="text-right">Price</TableHead>
                  <TableHead>Stock</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {products.data.items.map((p) => (
                  <TableRow key={p.productId}>
                    <TableCell>
                      <ProductImage src={p.imageUrl} alt={p.name} className="size-12 rounded-md" />
                    </TableCell>
                    <TableCell className="max-w-56 truncate font-medium">{p.name}</TableCell>
                    <TableCell>
                      <Badge variant="secondary">{p.categoryName}</Badge>
                    </TableCell>
                    <TableCell className="text-right tabular-nums">{formatCurrency(p.price)}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <span className="w-8 tabular-nums">{p.stockQuantity}</span>
                        <StockBadge quantity={p.stockQuantity} />
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex justify-end gap-1">
                        <Button
                          variant="ghost"
                          size="icon-sm"
                          aria-label={`Edit ${p.name}`}
                          onClick={() => {
                            setEditing(p);
                            setFormOpen(true);
                          }}
                        >
                          <Pencil />
                        </Button>
                        <Button variant="ghost" size="icon-sm" aria-label={`Delete ${p.name}`} onClick={() => setDeleting(p)}>
                          <Trash2 className="text-destructive" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </Card>
        )
      )}

      {products.data && (
        <Pagination
          page={products.data.pageNumber}
          totalPages={products.data.totalPages}
          totalCount={products.data.totalCount}
          disabled={products.isFetching}
          onPageChange={setPage}
        />
      )}

      {/* key remounts the form so defaults follow the product being edited */}
      <ProductFormDialog
        key={editing?.productId ?? "new"}
        open={formOpen}
        onOpenChange={setFormOpen}
        product={editing}
      />

      <ConfirmDialog
        open={!!deleting}
        onOpenChange={(open) => !open && setDeleting(null)}
        title="Delete this product?"
        description={
          <>
            <span className="font-medium text-foreground">{deleting?.name}</span> will be removed from the storefront. Past
            orders keep their records (soft delete).
          </>
        }
        pending={remove.isPending}
        onConfirm={confirmDelete}
      />
    </div>
  );
}
