import type {
  PaginatedResult,
  ProductCreateInput,
  ProductReadDto,
  ProductUpdateDto,
  QueryParameters,
} from "@/types/api";
import { http } from "./client";

export const productsApi = {
  async list(params: QueryParameters = {}) {
    const { data } = await http.get<PaginatedResult<ProductReadDto>>("/products", { params });
    return data;
  },

  async get(productId: string) {
    const { data } = await http.get<ProductReadDto>(`/products/${productId}`);
    return data;
  },

  /** multipart/form-data — field names mirror ProductCreateDto. */
  async create(input: ProductCreateInput) {
    const form = new FormData();
    form.append("Name", input.name);
    form.append("Price", String(input.price));
    form.append("StockQuantity", String(input.stockQuantity));
    form.append("CategoryId", input.categoryId);
    if (input.image) form.append("Image", input.image);
    const { data } = await http.post<ProductReadDto>("/products", form);
    return data;
  },

  async update(productId: string, dto: ProductUpdateDto) {
    const { data } = await http.put<ProductReadDto>(`/products/${productId}`, dto);
    return data;
  },

  /** Soft delete on the backend. */
  async remove(productId: string) {
    await http.delete(`/products/${productId}`);
  },
};
