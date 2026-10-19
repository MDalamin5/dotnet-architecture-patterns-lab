import type { SalesOverviewDto, TopSellingProductDto } from "@/types/api";
import { http } from "./client";

export const analyticsApi = {
  async salesOverview() {
    const { data } = await http.get<SalesOverviewDto>("/analytics/sales-overview");
    return data;
  },

  async topSellingProducts(count = 5) {
    const { data } = await http.get<TopSellingProductDto[]>("/analytics/top-selling-products", { params: { count } });
    return data;
  },
};
