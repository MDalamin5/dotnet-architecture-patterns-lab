"use client";

import { useState } from "react";
import { Info, Pencil, Plus, Tags, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { useCategories, useCategoryMutations } from "@/hooks/use-catalog";
import { getErrorMessage } from "@/lib/errors";
import { formatDate } from "@/lib/format";
import { useMerchantAuthStore } from "@/stores/auth";
import type { CategoryReadDto } from "@/types/api";
import { CategoryFormDialog } from "@/components/admin/category-form-dialog";
import { PageHeader } from "@/components/admin/page-header";
import { ConfirmDialog } from "@/components/common/confirm-dialog";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorAlert } from "@/components/common/error-alert";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

export default function AdminCategoriesPage() {
  const can = {
    create: useMerchantAuthStore((s) => s.hasPermission("categories.create")),
    update: useMerchantAuthStore((s) => s.hasPermission("categories.update")),
    delete: useMerchantAuthStore((s) => s.hasPermission("categories.delete")),
  };
  const categories = useCategories();
  const { remove } = useCategoryMutations();

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<CategoryReadDto | null>(null);
  const [deleting, setDeleting] = useState<CategoryReadDto | null>(null);

  const missing = (allowed: boolean, code: string) => (allowed ? undefined : `You need the ${code} permission`);

  function confirmDelete() {
    if (!deleting) return;
    remove.mutate(deleting.categoryId, {
      onSuccess: () => {
        toast.success(`“${deleting.name}” deleted`);
        setDeleting(null);
      },
      onError: (err) => toast.error(getErrorMessage(err, "Couldn't delete the category")),
    });
  }

  return (
    <div className="mx-auto max-w-4xl">
      <PageHeader
        title="Categories"
        description="Group products so customers can filter the storefront."
        actions={
          <Button
            disabled={!can.create}
            title={missing(can.create, "categories.create")}
            onClick={() => {
              setEditing(null);
              setFormOpen(true);
            }}
          >
            <Plus /> Add category
          </Button>
        }
      />

      <Alert className="mb-4">
        <Info />
        <AlertDescription>Updates will automatically purge the Redis cache.</AlertDescription>
      </Alert>

      <ErrorAlert error={categories.error} title="Couldn't load categories" className="mb-4" />

      {categories.isPending ? (
        <div className="space-y-2">
          {Array.from({ length: 4 }, (_, i) => (
            <Skeleton key={i} className="h-12" />
          ))}
        </div>
      ) : categories.data && categories.data.length === 0 ? (
        <EmptyState icon={Tags} title="No categories yet" description="Create a category before adding products." />
      ) : (
        categories.data && (
          <Card className="overflow-hidden py-0">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Name</TableHead>
                  <TableHead className="hidden sm:table-cell">Description</TableHead>
                  <TableHead className="hidden md:table-cell">Created</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {categories.data.map((c) => (
                  <TableRow key={c.categoryId}>
                    <TableCell className="font-medium">{c.name}</TableCell>
                    <TableCell className="hidden max-w-xs truncate text-muted-foreground sm:table-cell">{c.description}</TableCell>
                    <TableCell className="hidden text-muted-foreground md:table-cell">{formatDate(c.createdAt)}</TableCell>
                    <TableCell>
                      <div className="flex justify-end gap-1">
                        <Button
                          variant="ghost"
                          size="icon-sm"
                          aria-label={`Edit ${c.name}`}
                          disabled={!can.update}
                          title={missing(can.update, "categories.update")}
                          onClick={() => {
                            setEditing(c);
                            setFormOpen(true);
                          }}
                        >
                          <Pencil />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon-sm"
                          aria-label={`Delete ${c.name}`}
                          disabled={!can.delete}
                          title={missing(can.delete, "categories.delete")}
                          onClick={() => setDeleting(c)}
                        >
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

      <CategoryFormDialog key={editing?.categoryId ?? "new"} open={formOpen} onOpenChange={setFormOpen} category={editing} />

      <ConfirmDialog
        open={!!deleting}
        onOpenChange={(open) => !open && setDeleting(null)}
        title="Delete this category?"
        description={
          <>
            <span className="font-medium text-foreground">{deleting?.name}</span> will be removed. Products in it may need to
            be reassigned.
          </>
        }
        pending={remove.isPending}
        onConfirm={confirmDelete}
      />
    </div>
  );
}
