import { useMutation } from "@tanstack/react-query";
import { authApi, buildAuthUser } from "@/lib/api";
import type { AuthLoginDto, AuthRegisterDto } from "@/types/api";
import { useAuthStore } from "@/stores/auth";

/**
 * Login, then enrich with /auth/me (permissions). The token must be in the store before /auth/me
 * so the request interceptor attaches it — but we pass it explicitly and only commit on success.
 */
export function useLogin({ tenant, merchantOnly = false }: { tenant: boolean; merchantOnly?: boolean }) {
  const setAuth = useAuthStore((s) => s.setAuth);
  return useMutation({
    mutationFn: async (dto: AuthLoginDto) => {
      const auth = await authApi.login(dto, { tenant });
      if (merchantOnly && auth.role !== "Admin") {
        throw new Error("This account is not a store owner. Use the storefront to sign in as a customer.");
      }
      const user = await buildAuthUser(auth);
      setAuth(auth.token, user);
      return user;
    },
  });
}

export function useRegister() {
  const setAuth = useAuthStore((s) => s.setAuth);
  return useMutation({
    mutationFn: async (dto: AuthRegisterDto) => {
      const auth = await authApi.register({ ...dto, role: 1 });
      const user = await buildAuthUser(auth);
      setAuth(auth.token, user);
      return user;
    },
  });
}
