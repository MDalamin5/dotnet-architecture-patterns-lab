/**
 * TypeScript mirrors of the ASP.NET Core DTOs (TEcommerceWebApi/DTOs).
 * Guid -> string, DateTime -> ISO string, decimal -> number. Property names are camelCase
 * because ASP.NET serialises with the default camelCase policy.
 */

// ---- Envelope & paging -------------------------------------------------------------------

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[] | null;
  statusCode: number;
  timeStamp: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

/** QueryParameters.cs — the backend clamps pageSize to a max of 6. */
export type SortOrder = "NameAsc" | "NameDesc" | "CreatedAtAsc" | "CreatedAtDesc";

export interface QueryParameters {
  pageNumber?: number;
  pageSize?: number;
  searchValue?: string;
  sortOrder?: SortOrder;
}

// ---- Auth --------------------------------------------------------------------------------

/** Enums/UserRole.cs */
export const UserRole = { Customer: 1, Admin: 2 } as const;

export interface AuthLoginDto {
  email: string;
  password: string;
}

export interface AuthRegisterDto {
  email: string;
  fullName: string;
  password: string;
  role?: (typeof UserRole)[keyof typeof UserRole];
  tenantId?: string;
}

export interface AuthResponseDto {
  token: string;
  userId: string;
  email: string;
  fullName: string;
  role: string;
  expiresAt: string;
}

/** GET /auth/me */
export interface CurrentUserDto {
  userId: string;
  tenantId: string | null;
  email: string;
  fullName: string;
  role: string;
  permissions: string[];
}

// ---- Tenants -----------------------------------------------------------------------------

export interface TenantCreateDto {
  storeName: string;
  subdomain: string;
  customDomain?: string;
  ownerEmail: string;
  ownerFullName: string;
  ownerPassword: string;
}

export interface TenantReadDto {
  tenantId: string;
  storeName: string;
  subdomain: string;
  customDomain: string | null;
  ownerEmail: string;
  isActive: boolean;
  createdAt: string;
}

// ---- Categories --------------------------------------------------------------------------

export interface CategoryReadDto {
  categoryId: string;
  name: string | null;
  description: string | null;
  createdAt: string;
}

export interface CategoryCreateDto {
  name: string; // 2-20 chars
  description: string; // <= 500 chars
}

export interface CategoryUpdateDto {
  name: string; // 2-20 chars
  description: string; // required
}

// ---- Products ----------------------------------------------------------------------------

export interface ProductReadDto {
  productId: string;
  name: string;
  price: number;
  imageUrl: string | null;
  stockQuantity: number;
  categoryId: string;
  categoryName: string;
}

/** Sent as multipart/form-data; field names match ProductCreateDto (model binding is case-insensitive). */
export interface ProductCreateInput {
  name: string;
  price: number;
  stockQuantity: number;
  categoryId: string;
  image?: File | null;
}

/** PUT /products/{id} is JSON and cannot change the image. */
export interface ProductUpdateDto {
  name: string;
  price: number;
  stockQuantity: number;
  categoryId: string;
}

// ---- Orders ------------------------------------------------------------------------------

/** Enums/OrderStatus.cs — OrderReadDto.status is serialised as the enum name. */
export type OrderStatus = "Pending" | "Processing" | "Completed" | "Cancelled";

export interface OrderItemCheckoutDto {
  productId: string;
  quantity: number; // 1-100
}

export interface OrderCheckoutDto {
  items: OrderItemCheckoutDto[];
}

export interface OrderItemReadDto {
  orderItemId: string;
  productId: string;
  productName: string;
  categoryName: string;
  quantity: number;
  unitPrice: number;
  subTotal: number;
}

export interface OrderReadDto {
  orderId: string;
  orderDate: string;
  totalAmount: number;
  status: OrderStatus | string;
  customerName: string;
  customerEmail: string;
  items: OrderItemReadDto[];
}

// ---- Analytics ---------------------------------------------------------------------------

export interface SalesOverviewDto {
  totalRevenue: number;
  totalOrdersCount: number;
  totalCustomersCount: number;
  totalActiveProducts: number;
  lowStockProductsCount: number;
}

export interface TopSellingProductDto {
  productId: string;
  productName: string;
  categoryName: string;
  totalUnitsSold: number;
  totalRevenue: number;
}

// ---- Roles & permissions (PBAC) ----------------------------------------------------------

export interface PermissionReadDto {
  permissionId: string;
  code: string; // e.g. "products.create"
  description: string;
}

export interface RoleReadDto {
  roleId: string;
  name: string;
  description: string;
  permissions: PermissionReadDto[];
}

export interface RoleCreateDto {
  name: string; // 2-50 chars
  description: string; // <= 200 chars
  permissionIds: string[];
}

export interface UpdateRolePermissionsDto {
  permissionIds: string[];
}
