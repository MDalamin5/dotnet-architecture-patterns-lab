"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { useCategoryMutations } from "@/hooks/use-catalog";
import type { CategoryReadDto } from "@/types/api";
import { ErrorAlert } from "@/components/common/error-alert";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";

// CategoryCreateDto: name 2-20, description <= 500. CategoryUpdateDto additionally requires a description.
const schema = z.object({
  name: z.string().trim().min(2, "At least 2 characters").max(20, "At most 20 characters"),
  description: z.string().trim().max(500, "At most 500 characters"),
});
type Values = z.infer<typeof schema>;

interface CategoryFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  category?: CategoryReadDto | null;
}

export function CategoryFormDialog({ open, onOpenChange, category }: CategoryFormDialogProps) {
  const editing = !!category;
  const { create, update } = useCategoryMutations();
  const mutation = editing ? update : create;
  const form = useForm<Values>({
    resolver: zodResolver(
      editing ? schema.extend({ description: z.string().trim().min(1, "Description is required").max(500) }) : schema,
    ),
    defaultValues: { name: category?.name ?? "", description: category?.description ?? "" },
  });
  const { errors } = form.formState;

  function close(next: boolean) {
    if (!next) {
      form.reset();
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
    if (category) update.mutate({ id: category.categoryId, dto: values }, done("Category updated"));
    else create.mutate(values, done("Category created"));
  }

  return (
    <Dialog open={open} onOpenChange={close}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{editing ? "Edit category" : "Add category"}</DialogTitle>
          <DialogDescription>Saving purges the cached category list automatically.</DialogDescription>
        </DialogHeader>
        <form className="space-y-4" onSubmit={form.handleSubmit(submit)}>
          <div className="space-y-1.5">
            <Label htmlFor="c-name">Name</Label>
            <Input id="c-name" {...form.register("name")} />
            {errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="c-desc">Description</Label>
            <Textarea id="c-desc" rows={3} {...form.register("description")} />
            {errors.description && <p className="text-xs text-destructive">{errors.description.message}</p>}
          </div>
          <ErrorAlert error={mutation.error} title="Couldn't save the category" />
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => close(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Saving…" : "Save"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
