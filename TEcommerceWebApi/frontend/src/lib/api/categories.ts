import type {
  CategoryCreateDto,
  CategoryReadDto,
  CategoryUpdateDto,
  PaginatedResult,
  QueryParameters,
} from "@/types/api";
import { http } from "./client";

export const categoriesApi = {
  async list(params: QueryParameters = {}) {
    const { data } = await http.get<PaginatedResult<CategoryReadDto>>("/categories", { params });
    return data;
  },

  /** The backend clamps pageSize to 6, so walk every page to get the full list. */
  async listAll() {
    const all: CategoryReadDto[] = [];
    let pageNumber = 1;
    let totalPages = 1;
    do {
      const page = await categoriesApi.list({ pageNumber, pageSize: 6 });
      all.push(...page.items);
      totalPages = page.totalPages;
      pageNumber += 1;
    } while (pageNumber <= totalPages);
    return all;
  },

  async create(dto: CategoryCreateDto) {
    const { data } = await http.post<CategoryReadDto>("/categories", dto);
    return data;
  },

  async update(categoryId: string, dto: CategoryUpdateDto) {
    const { data } = await http.put<CategoryReadDto>(`/categories/${categoryId}`, dto);
    return data;
  },

  async remove(categoryId: string) {
    await http.delete(`/categories/${categoryId}`);
  },
};
