import type { PermissionReadDto, RoleCreateDto, RoleReadDto, UpdateRolePermissionsDto } from "@/types/api";
import { http } from "./client";

export const rolesApi = {
  async permissions() {
    const { data } = await http.get<PermissionReadDto[]>("/roles/permissions");
    return data;
  },

  async list() {
    const { data } = await http.get<RoleReadDto[]>("/roles");
    return data;
  },

  async create(dto: RoleCreateDto) {
    const { data } = await http.post<RoleReadDto>("/roles", dto);
    return data;
  },

  async updatePermissions(roleId: string, dto: UpdateRolePermissionsDto) {
    const { data } = await http.put<RoleReadDto>(`/roles/${roleId}/permissions`, dto);
    return data;
  },
};
