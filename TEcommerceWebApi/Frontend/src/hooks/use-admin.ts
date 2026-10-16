import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { analyticsApi, rolesApi, tenantsApi } from "@/lib/api";
import type { RoleCreateDto, TenantCreateDto, UpdateRolePermissionsDto } from "@/types/api";
import { useAuthStore } from "@/stores/auth";
import { useTenantId } from "./use-catalog";

export function useMyStores() {
  const userId = useAuthStore((s) => s.user?.userId);
  return useQuery({
    queryKey: ["tenants", "mine", userId],
    queryFn: () => tenantsApi.myStores(),
    enabled: !!userId,
  });
}

export function useCreateStore() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: TenantCreateDto) => tenantsApi.create(dto),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["tenants", "mine"] }),
  });
}

export function useSalesOverview() {
  const tenantId = useTenantId();
  return useQuery({
    queryKey: ["analytics", "overview", tenantId],
    queryFn: () => analyticsApi.salesOverview(),
    enabled: !!tenantId,
  });
}

export function useTopSellingProducts(count = 5) {
  const tenantId = useTenantId();
  return useQuery({
    queryKey: ["analytics", "top-selling", tenantId, count],
    queryFn: () => analyticsApi.topSellingProducts(count),
    enabled: !!tenantId,
  });
}

export function useRoles() {
  const tenantId = useTenantId();
  return useQuery({ queryKey: ["roles", tenantId], queryFn: () => rolesApi.list(), enabled: !!tenantId });
}

export function usePermissions() {
  const tenantId = useTenantId();
  return useQuery({
    queryKey: ["permissions", tenantId],
    queryFn: () => rolesApi.permissions(),
    enabled: !!tenantId,
    staleTime: 5 * 60_000,
  });
}

export function useRoleMutations() {
  const qc = useQueryClient();
  const invalidate = () => qc.invalidateQueries({ queryKey: ["roles"] });
  return {
    create: useMutation({ mutationFn: (dto: RoleCreateDto) => rolesApi.create(dto), onSuccess: invalidate }),
    updatePermissions: useMutation({
      mutationFn: ({ id, dto }: { id: string; dto: UpdateRolePermissionsDto }) => rolesApi.updatePermissions(id, dto),
      onSuccess: invalidate,
    }),
  };
}
