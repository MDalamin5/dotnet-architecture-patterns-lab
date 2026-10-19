import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ordersApi } from "@/lib/api";
import type { OrderCheckoutDto } from "@/types/api";
import { useAuthStore } from "@/stores/auth";
import { useTenantId } from "./use-catalog";

export function useMyOrders() {
  const tenantId = useTenantId();
  const userId = useAuthStore((s) => s.user?.userId);
  return useQuery({
    queryKey: ["orders", "mine", tenantId, userId],
    queryFn: () => ordersApi.myOrders(),
    enabled: !!tenantId && !!userId,
  });
}

export function useOrder(orderId: string) {
  const tenantId = useTenantId();
  const userId = useAuthStore((s) => s.user?.userId);
  return useQuery({
    queryKey: ["orders", "detail", tenantId, userId, orderId],
    queryFn: () => ordersApi.get(orderId),
    enabled: !!tenantId && !!userId,
  });
}

export function useCheckout() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (dto: OrderCheckoutDto) => ordersApi.checkout(dto),
    onSuccess: () => {
      // Stock levels and the order list both changed.
      void qc.invalidateQueries({ queryKey: ["orders"] });
      void qc.invalidateQueries({ queryKey: ["products"] });
    },
  });
}
