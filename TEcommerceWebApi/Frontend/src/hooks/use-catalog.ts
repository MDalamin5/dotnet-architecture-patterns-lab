import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { categoriesApi, productsApi } from "@/lib/api";
import type { CategoryCreateDto, CategoryUpdateDto, ProductCreateInput, ProductUpdateDto, QueryParameters } from "@/types/api";
import { usePathname } from "next/navigation";
import { isAdminPath } from "@/stores/session";
import { useStorefrontTenantStore, useTenantStore } from "@/stores/tenant";

/** The tenant the current portal is working against (merchant: active store; customer: browsed store). */
export function useTenantId() {
  const admin = isAdminPath(usePathname());
  const merchantTenant = useTenantStore((s) => s.activeTenant?.tenantId ?? null);
  const storefrontTenant = useStorefrontTenantStore((s) => s.tenant?.tenantId ?? null);
  return admin ? merchantTenant : storefrontTenant;
}

export function useProducts(params: QueryParameters) {
  const tenantId = useTenantId();
  return useQuery({
    queryKey: ["products", tenantId, params],
    queryFn: () => productsApi.list(params),
    enabled: !!tenantId,
    placeholderData: keepPreviousData,
  });
}

export function useCategories() {
  const tenantId = useTenantId();
  return useQuery({
    queryKey: ["categories", tenantId],
    queryFn: () => categoriesApi.listAll(),
    enabled: !!tenantId,
    staleTime: 60_000,
  });
}

export function useProductMutations() {
  const qc = useQueryClient();
  const invalidate = () => qc.invalidateQueries({ queryKey: ["products"] });
  return {
    create: useMutation({ mutationFn: (input: ProductCreateInput) => productsApi.create(input), onSuccess: invalidate }),
    update: useMutation({
      mutationFn: ({ id, dto }: { id: string; dto: ProductUpdateDto }) => productsApi.update(id, dto),
      onSuccess: invalidate,
    }),
    remove: useMutation({ mutationFn: (id: string) => productsApi.remove(id), onSuccess: invalidate }),
  };
}

export function useCategoryMutations() {
  const qc = useQueryClient();
  // Product rows embed categoryName, so refresh both.
  const invalidate = () => Promise.all([
    qc.invalidateQueries({ queryKey: ["categories"] }),
    qc.invalidateQueries({ queryKey: ["products"] }),
  ]);
  return {
    create: useMutation({ mutationFn: (dto: CategoryCreateDto) => categoriesApi.create(dto), onSuccess: invalidate }),
    update: useMutation({
      mutationFn: ({ id, dto }: { id: string; dto: CategoryUpdateDto }) => categoriesApi.update(id, dto),
      onSuccess: invalidate,
    }),
    remove: useMutation({ mutationFn: (id: string) => categoriesApi.remove(id), onSuccess: invalidate }),
  };
}
