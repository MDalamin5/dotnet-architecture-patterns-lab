import type { OrderCheckoutDto, OrderReadDto, OrderStatus } from "@/types/api";
import { http } from "./client";

export const ordersApi = {
  async checkout(dto: OrderCheckoutDto) {
    const { data } = await http.post<OrderReadDto>("/orders/checkout", dto);
    return data;
  },

  async myOrders(status?: OrderStatus) {
    const { data } = await http.get<OrderReadDto[]>("/orders/my-orders", { params: status ? { status } : undefined });
    return data;
  },

  async get(orderId: string) {
    const { data } = await http.get<OrderReadDto>(`/orders/${orderId}`);
    return data;
  },
};
