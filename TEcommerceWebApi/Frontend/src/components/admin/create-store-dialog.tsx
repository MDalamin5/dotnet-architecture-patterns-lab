"use client";

import { useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { useCreateStore } from "@/hooks/use-admin";
import { useMerchantAuthStore } from "@/stores/auth";
import { env } from "@/lib/env";
import { ErrorAlert } from "@/components/common/error-alert";
import { FormField } from "@/components/common/form-field";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";

// Mirrors TenantCreateDto validation.
const schema = z.object({
  storeName: z.string().min(2, "At least 2 characters").max(100),
  subdomain: z
    .string()
    .min(3, "At least 3 characters")
    .max(50)
    .regex(/^[a-z0-9-]+$/, "Lowercase letters, numbers and hyphens only"),
  customDomain: z.string().optional(),
  ownerEmail: z.string().email("Enter a valid email"),
  ownerFullName: z.string().min(2, "At least 2 characters").max(100),
  ownerPassword: z.string().min(6, "At least 6 characters"),
});
type Values = z.infer<typeof schema>;

export function CreateStoreDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (open: boolean) => void }) {
  const user = useMerchantAuthStore((s) => s.user);
  const create = useCreateStore();
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: { ownerEmail: user?.email ?? "", ownerFullName: user?.fullName ?? "" },
  });
  const { errors } = form.formState;
  const subdomain = useWatch({ control: form.control, name: "subdomain" });

  function close(next: boolean) {
    if (!next) {
      form.reset();
      create.reset();
    }
    onOpenChange(next);
  }

  return (
    <Dialog open={open} onOpenChange={close}>
      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Create a new store</DialogTitle>
          <DialogDescription>This also creates the store&apos;s admin account.</DialogDescription>
        </DialogHeader>

        <form
          className="space-y-4"
          onSubmit={form.handleSubmit((values) =>
            create.mutate(
              { ...values, customDomain: values.customDomain?.trim() || undefined },
              {
                onSuccess: (store) => {
                  toast.success(`Store “${store.storeName}” created`);
                  close(false);
                },
              },
            ),
          )}
        >
          <FormField label="Store name" error={errors.storeName?.message}>
            <Input {...form.register("storeName")} />
          </FormField>
          <FormField
            label="Subdomain"
            error={errors.subdomain?.message}
            hint={`${subdomain || "your-store"}.${env.rootDomain}`}
          >
            <Input autoCapitalize="none" {...form.register("subdomain")} />
          </FormField>
          <FormField label="Custom domain (optional)" error={errors.customDomain?.message}>
            <Input placeholder="www.mystore.com" {...form.register("customDomain")} />
          </FormField>

          <fieldset className="space-y-4 rounded-lg border p-3">
            <legend className="px-1 text-xs text-muted-foreground">Store owner</legend>
            <FormField
              label="Owner email"
              error={errors.ownerEmail?.message}
              hint="Use your own login email so the store shows up in your list."
            >
              <Input type="email" {...form.register("ownerEmail")} />
            </FormField>
            <FormField label="Owner full name" error={errors.ownerFullName?.message}>
              <Input {...form.register("ownerFullName")} />
            </FormField>
            <FormField label="Owner password" error={errors.ownerPassword?.message}>
              <Input type="password" autoComplete="new-password" {...form.register("ownerPassword")} />
            </FormField>
          </fieldset>

          <ErrorAlert error={create.error} title="Couldn't create the store" />
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => close(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={create.isPending}>
              {create.isPending ? "Creating…" : "Create store"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
