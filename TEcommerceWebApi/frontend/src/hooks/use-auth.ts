import { useMutation } from "@tanstack/react-query";
import { authApi, buildAuthUser } from "@/lib/api";
import type { AuthLoginDto, AuthRegisterDto } from "@/types/api";
import { useAuthStore, useMerchantAuthStore } from "@/stores/auth";

/**
 * Login, then enrich with /auth/me (permissions). /auth/me gets the fresh token explicitly, and the
 * session is only committed once everything succeeded.
 */
export function useLogin({ portal }: { portal: "admin" | "store" }) {
  const merchant = portal === "admin";
  const setCustomerAuth = useAuthStore((s) => s.setAuth);
  const setMerchantAuth = useMerchantAuthStore((s) => s.setAuth);
  const setAuth = merchant ? setMerchantAuth : setCustomerAuth;
  return useMutation({
    mutationFn: async (dto: AuthLoginDto) => {
      // Merchants log in "globally" (no X-Tenant-Id); customers log in to the storefront's tenant.
      const auth = await authApi.login(dto, { tenant: !merchant });
      if (merchant && auth.role !== "Admin") {
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
