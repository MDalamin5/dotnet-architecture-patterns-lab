"use client";

import { useMemo, useState } from "react";
import { toast } from "sonner";
import { useRoleMutations } from "@/hooks/use-admin";
import { humanizePermission } from "@/lib/format";
import type { PermissionReadDto, RoleReadDto } from "@/types/api";
import { ErrorAlert } from "@/components/common/error-alert";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Sheet, SheetContent, SheetDescription, SheetFooter, SheetHeader, SheetTitle } from "@/components/ui/sheet";
import { Textarea } from "@/components/ui/textarea";

/** "orders.manage_status" belongs to the "Orders" group. */
const groupOf = (p: PermissionReadDto) => p.code.split(".")[0];
const groupLabel = (g: string) => g.charAt(0).toUpperCase() + g.slice(1);

interface RoleSheetProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  permissions: PermissionReadDto[];
  /** Edit an existing role's permissions; omit to create a custom role. */
  role?: RoleReadDto | null;
}

export function RoleSheet({ open, onOpenChange, permissions, role }: RoleSheetProps) {
  const editing = !!role;
  const { create, updatePermissions } = useRoleMutations();
  const mutation = editing ? updatePermissions : create;

  const [name, setName] = useState(role?.name ?? "");
  const [description, setDescription] = useState(role?.description ?? "");
  const [selected, setSelected] = useState<Set<string>>(() => new Set(role?.permissions.map((p) => p.permissionId)));
  const [touched, setTouched] = useState(false);

  const groups = useMemo(() => {
    const map = new Map<string, PermissionReadDto[]>();
    for (const p of permissions) {
      const g = groupOf(p);
      map.set(g, [...(map.get(g) ?? []), p]);
    }
    return [...map.entries()];
  }, [permissions]);

  const nameError = !editing && touched && (name.trim().length < 2 || name.trim().length > 50) ? "Role name must be 2–50 characters." : null;
  const descError = description.length > 200 ? "Description can be at most 200 characters." : null;

  function toggle(ids: string[], on: boolean) {
    setSelected((prev) => {
      const next = new Set(prev);
      for (const id of ids) {
        if (on) next.add(id);
        else next.delete(id);
      }
      return next;
    });
  }

  function close(next: boolean) {
    if (!next) {
      create.reset();
      updatePermissions.reset();
    }
    onOpenChange(next);
  }

  function submit(e: React.FormEvent) {
    e.preventDefault();
    setTouched(true);
    if (descError || (!editing && (name.trim().length < 2 || name.trim().length > 50))) return;
    const permissionIds = [...selected];
    const done = (message: string) => ({
      onSuccess: () => {
        toast.success(message);
        close(false);
      },
    });
    if (role) updatePermissions.mutate({ id: role.roleId, dto: { permissionIds } }, done(`“${role.name}” permissions updated`));
    else create.mutate({ name: name.trim(), description: description.trim(), permissionIds }, done(`Role “${name.trim()}” created`));
  }

  return (
    <Sheet open={open} onOpenChange={close}>
      <SheetContent className="w-full sm:max-w-lg">
        <form onSubmit={submit} className="flex h-full min-h-0 flex-col">
          <SheetHeader className="border-b">
            <SheetTitle>{editing ? `Edit ${role.name}` : "Create custom role"}</SheetTitle>
            <SheetDescription>
              {editing ? "Choose what this role is allowed to do." : "Name the role, then tick the permissions it grants."}
            </SheetDescription>
          </SheetHeader>

          <div className="flex-1 space-y-5 overflow-y-auto p-4">
            {!editing && (
              <>
                <div className="space-y-1.5">
                  <Label htmlFor="r-name">Role name</Label>
                  <Input id="r-name" placeholder="InventoryManager" value={name} onChange={(e) => setName(e.target.value)} />
                  {nameError && <p className="text-xs text-destructive">{nameError}</p>}
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="r-desc">Description</Label>
                  <Textarea id="r-desc" rows={2} value={description} onChange={(e) => setDescription(e.target.value)} />
                  {descError && <p className="text-xs text-destructive">{descError}</p>}
                </div>
              </>
            )}

            <div className="space-y-4">
              <p className="text-sm font-medium">
                Permissions <span className="font-normal text-muted-foreground">({selected.size} selected)</span>
              </p>
              {groups.map(([group, perms]) => {
                const ids = perms.map((p) => p.permissionId);
                const all = ids.every((id) => selected.has(id));
                return (
                  <fieldset key={group} className="rounded-lg border">
                    <legend className="sr-only">{groupLabel(group)} permissions</legend>
                    <div className="flex items-center justify-between border-b bg-muted/50 px-3 py-2">
                      <span className="text-sm font-medium">{groupLabel(group)}</span>
                      <button
                        type="button"
                        className="text-xs text-primary hover:underline"
                        onClick={() => toggle(ids, !all)}
                      >
                        {all ? "Clear" : "Select all"}
                      </button>
                    </div>
                    <div className="grid gap-1 p-2 sm:grid-cols-2">
                      {perms.map((p) => (
                        <label
                          key={p.permissionId}
                          className="flex cursor-pointer items-start gap-2 rounded-md p-2 text-sm hover:bg-muted"
                        >
                          <Checkbox
                            className="mt-0.5"
                            checked={selected.has(p.permissionId)}
                            onCheckedChange={(checked) => toggle([p.permissionId], checked)}
                          />
                          <span>
                            <span className="block font-medium">{humanizePermission(p.code)}</span>
                            <span className="block text-xs text-muted-foreground">{p.description}</span>
                          </span>
                        </label>
                      ))}
                    </div>
                  </fieldset>
                );
              })}
            </div>

            <ErrorAlert error={mutation.error} title={editing ? "Couldn't update permissions" : "Couldn't create the role"} />
          </div>

          <SheetFooter className="border-t sm:flex-row sm:justify-end">
            <Button type="button" variant="outline" onClick={() => close(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Saving…" : editing ? "Save permissions" : "Create role"}
            </Button>
          </SheetFooter>
        </form>
      </SheetContent>
    </Sheet>
  );
}
