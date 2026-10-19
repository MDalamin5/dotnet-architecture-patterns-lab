import type { AuthLoginDto, AuthRegisterDto, AuthResponseDto, CurrentUserDto } from "@/types/api";
import type { AuthUser } from "@/stores/auth";
import { http } from "./client";

export const authApi = {
  /** `tenant: false` = merchant "global" login (no X-Tenant-Id); the JWT carries the TenantId claim. */
  async login(dto: AuthLoginDto, { tenant = true }: { tenant?: boolean } = {}) {
    const { data } = await http.post<AuthResponseDto>("/auth/login", dto, {
      skipTenant: !tenant,
      skipAuthRedirect: true,
    });
    return data;
  },

  async register(dto: AuthRegisterDto) {
    const { data } = await http.post<AuthResponseDto>("/auth/register", dto, { skipAuthRedirect: true });
    return data;
  },

  async me(token?: string) {
    const { data } = await http.get<CurrentUserDto>("/auth/me", {
      headers: token ? { Authorization: `Bearer ${token}` } : undefined,
      skipAuthRedirect: true,
    });
    return data;
  },
};

/** AuthResponseDto doesn't include permissions, so enrich it from /auth/me. */
export async function buildAuthUser(auth: AuthResponseDto): Promise<AuthUser> {
  const me = await authApi.me(auth.token);
  return {
    userId: auth.userId,
    email: auth.email,
    fullName: auth.fullName,
    role: auth.role,
    tenantId: me.tenantId ?? undefined,
    permissions: me.permissions ?? [],
  };
}
