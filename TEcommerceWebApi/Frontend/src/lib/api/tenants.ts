import type { TenantCreateDto, TenantReadDto } from "@/types/api";
import { http } from "./client";

export const tenantsApi = {
  async myStores() {
    const { data } = await http.get<TenantReadDto[]>("/tenants/my-stores", { skipTenant: true });
    return data;
  },

  async create(dto: TenantCreateDto) {
    const { data } = await http.post<TenantReadDto>("/tenants", dto, { skipTenant: true });
    return data;
  },

  /**
   * Resolve a subdomain (nike.byvstore.com) to a store.
   * NOTE: the backend does not expose this yet — it only resolves hosts internally. Until a public
   * `GET /tenants/by-subdomain/{subdomain}` exists, storefront URLs must use the tenant GUID
   * (/store/<tenantId>), which is what the admin "Visit storefront" link generates.
   */
  async bySubdomain(subdomain: string) {
    const { data } = await http.get<TenantReadDto>(`/tenants/by-subdomain/${encodeURIComponent(subdomain)}`, {
      skipTenant: true,
    });
    return data;
  },
};
