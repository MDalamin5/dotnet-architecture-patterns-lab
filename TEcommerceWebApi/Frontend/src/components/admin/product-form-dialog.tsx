"use client";

import { useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { useCategories, useProductMutations } from "@/hooks/use-catalog";
import type { ProductReadDto } from "@/types/api";
import { ErrorAlert } from "@/components/common/error-alert";
import { ProductImage } from "@/components/common/product-image";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { ImageDropzone } from "./image-dropzone";

// Mirrors ProductCreateDto / ProductUpdateDto validation.
const schema = z.object({
  name: z.string().min(2, "At least 2 characters").max(150, "At most 150 characters"),
  price: z.number({ error: "Enter a price" }).min(0.01, "Minimum is 0.01").max(100000, "Maximum is 100,000"),
  stockQuantity: z
    .number({ error: "Enter a quantity" })
    .int("Whole numbers only")
    .min(0, "Cannot be negative")
    .max(100000, "Maximum is 100,000"),
  categoryId: z.string().min(1, "Choose a category"),
});
type Values = z.infer<typeof schema>;

interface ProductFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  /** When set, the dialog edits this product (JSON PUT, no image); otherwise it creates (multipart POST). */
  product?: ProductReadDto | null;
}

function FieldError({ message }: { message?: string }) {
  return message ? <p className="text-xs text-destructive">{message}</p> : null;
}

export function ProductFormDialog({ open, onOpenChange, product }: ProductFormDialogProps) {
  const editing = !!product;
  const categories = useCategories();
  const { create, update } = useProductMutations();
  const mutation = editing ? update : create;
  const [image, setImage] = useState<File | null>(null);

  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: product?.name ?? "",
      price: product?.price,
      stockQuantity: product?.stockQuantity ?? 0,
      categoryId: product?.categoryId ?? "",
    },
  });
  const { errors } = form.formState;

  function close(next: boolean) {
    if (!next) {
      form.reset();
      setImage(null);
      create.reset();
      update.reset();
    }
    onOpenChange(next);
  }

  function submit(values: Values) {
    const done = (message: string) => ({
      onSuccess: () => {
        toast.success(message);
        close(false);
      },
    });
    if (product) {
      update.mutate({ id: product.productId, dto: values }, done("Product updated"));
    } else {
      create.mutate({ ...values, image }, done("Product created"));
    }
  }

  return (
    <Dialog open={open} onOpenChange={close}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{editing ? "Edit product" : "Add new product"}</DialogTitle>
          <DialogDescription>
            {editing ? "Update the product details." : "Upload an image and fill in the details."}
          </DialogDescription>
        </DialogHeader>

        <form className="space-y-4" onSubmit={form.handleSubmit(submit)}>
          {editing ? (
            <div className="flex items-center gap-3 rounded-lg bg-muted p-3">
              <ProductImage src={product.imageUrl} alt={product.name} className="size-14 shrink-0 rounded-md" />
              <p className="text-xs text-muted-foreground">
                The image is set when a product is created and can&apos;t be changed through the API yet.
              </p>
            </div>
          ) : (
            <ImageDropzone file={image} onChange={setImage} disabled={mutation.isPending} />
          )}

          <div className="space-y-1.5">
            <Label htmlFor="p-name">Name</Label>
            <Input id="p-name" {...form.register("name")} />
            <FieldError message={errors.name?.message} />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-1.5">
              <Label htmlFor="p-price">Price (USD)</Label>
              <Input id="p-price" type="number" step="0.01" inputMode="decimal" {...form.register("price", { valueAsNumber: true })} />
              <FieldError message={errors.price?.message} />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="p-stock">Stock quantity</Label>
              <Input id="p-stock" type="number" step="1" inputMode="numeric" {...form.register("stockQuantity", { valueAsNumber: true })} />
              <FieldError message={errors.stockQuantity?.message} />
            </div>
          </div>

          <div className="space-y-1.5">
            <Label>Category</Label>
            <Controller
              control={form.control}
              name="categoryId"
              render={({ field }) => (
                <Select
                  value={field.value}
                  onValueChange={(v) => field.onChange(v)}
                  items={(categories.data ?? []).map((c) => ({ value: c.categoryId, label: c.name ?? "Unnamed" }))}
                >
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder={categories.isPending ? "Loading categories…" : "Select a category"} />
                  </SelectTrigger>
                  <SelectContent>
                    {categories.data?.map((c) => (
                      <SelectItem key={c.categoryId} value={c.categoryId}>
                        {c.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
            <FieldError message={errors.categoryId?.message} />
            {categories.data?.length === 0 && (
              <p className="text-xs text-muted-foreground">No categories yet — create one on the Categories page first.</p>
            )}
          </div>

          <ErrorAlert error={mutation.error} title={editing ? "Couldn't update the product" : "Couldn't create the product"} />

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => close(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Saving…" : editing ? "Save changes" : "Create product"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
