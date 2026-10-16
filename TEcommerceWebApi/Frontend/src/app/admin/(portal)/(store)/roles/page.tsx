"use client";

import { useState } from "react";
import { Plus, ShieldCheck } from "lucide-react";
import { usePermissions, useRoles } from "@/hooks/use-admin";
import type { RoleReadDto } from "@/types/api";
import { PageHeader } from "@/components/admin/page-header";
import { RoleSheet } from "@/components/admin/role-sheet";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorAlert } from "@/components/common/error-alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

export default function RolesPage() {
  const roles = useRoles();
  const permissions = usePermissions();
  const [sheet, setSheet] = useState<{ open: boolean; role: RoleReadDto | null }>({ open: false, role: null });

  const loading = roles.isPending || permissions.isPending;

  return (
    <div className="mx-auto max-w-5xl">
      <PageHeader
        title="Roles & permissions"
        description="Permission-based access control: build roles from the permissions below."
        actions={
          <Button disabled={!permissions.data} onClick={() => setSheet({ open: true, role: null })}>
            <Plus /> Create custom role
          </Button>
        }
      />

      <ErrorAlert error={roles.error ?? permissions.error} title="Couldn't load roles" className="mb-4" />

      {loading ? (
        <div className="grid gap-4 md:grid-cols-2">
          {Array.from({ length: 2 }, (_, i) => (
            <Skeleton key={i} className="h-48 rounded-xl" />
          ))}
        </div>
      ) : roles.data && roles.data.length === 0 ? (
        <EmptyState icon={ShieldCheck} title="No roles" description="Create a custom role to get started." />
      ) : (
        <div className="grid gap-4 md:grid-cols-2">
          {roles.data?.map((role) => (
            <Card key={role.roleId}>
              <CardHeader>
                <CardTitle>{role.name}</CardTitle>
                <CardDescription>{role.description || "No description"}</CardDescription>
              </CardHeader>
              <CardContent className="flex flex-wrap gap-1.5">
                {role.permissions.length === 0 ? (
                  <p className="text-sm text-muted-foreground">No permissions assigned.</p>
                ) : (
                  role.permissions.map((p) => (
                    <Badge key={p.permissionId} variant="secondary" title={p.description}>
                      {p.code}
                    </Badge>
                  ))
                )}
              </CardContent>
              <CardFooter>
                <Button variant="outline" size="sm" disabled={!permissions.data} onClick={() => setSheet({ open: true, role })}>
                  Edit permissions
                </Button>
              </CardFooter>
            </Card>
          ))}
        </div>
      )}

      {permissions.data && (
        // key remounts the sheet so its form state always starts from the selected role
        <RoleSheet
          key={sheet.role?.roleId ?? "new"}
          open={sheet.open}
          onOpenChange={(open) => setSheet((s) => ({ ...s, open }))}
          permissions={permissions.data}
          role={sheet.role}
        />
      )}
    </div>
  );
}
