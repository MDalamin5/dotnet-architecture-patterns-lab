# 📋 Frontend Technical Specification Document (PRD)

> **Document Objective:** This specification provides the complete technical blueprint for generating the modern, multi-tenant frontend using **Claude Code**. It contains every route, state store, API integration contract, and UI component requirement.

---

## 1. Executive Architecture & Framework Selection

### Recommended Framework: **Next.js (App Router, TypeScript, Tailwind CSS, shadcn/ui)**

#### Why Next.js for this Multi-Tenant System?
1. **Multi-Tenant Subdomain Routing via Edge Middleware:** Next.js can intercept incoming domain names (`nike.byvstore.com` vs `apex.byvstore.com`) and rewrite requests to the correct tenant storefront internally.
2. **SEO & Performance for Public Storefronts:** Server-Side Rendering (SSR) ensures Google indexes each store’s product pages with metadata and structured data.
3. **shadcn/ui + Tailwind CSS:** Provides accessible UI components (dialogs, tables, drawers, dropdowns) with a minimal footprint.
4. **TanStack Query (React Query) + Zustand:** Manages API caching and cart/auth state with zero boilerplate.

### Complete Technology Stack:
* **Core:** Next.js 15+ (App Router), React 19, TypeScript 5+
* **Styling & Components:** Tailwind CSS, shadcn/ui, Lucide React Icons
* **Data Fetching:** TanStack Query v5, Axios (with auth/tenant interceptors)
* **Client State:** Zustand (with `persist` middleware for Cart and Auth sessions)
* **Forms & Validation:** React Hook Form + Zod

---

## 2. Multi-Tenant Domain & Routing Architecture

The application is divided into two distinct portals:
1. **The Merchant Admin Portal** (`app.byvstore.com` or `localhost:3000/admin`): Store owner hub with a store switcher, product management, MinIO uploads, and sales analytics.
2. **The White-Labeled Storefront** (`[subdomain].byvstore.com` or `localhost:3000/store`): Clean, branded customer experience with no store switcher or competitor branding.

```
Frontend Application Structure
├── middleware.ts (Extracts subdomain / host → injects X-Tenant-Id header)
├── app/
│   ├── (platform)/               # Merchant & Platform Routes
│   │   ├── admin/
│   │   │   ├── login/
│   │   │   ├── stores/          # Store Switcher & Onboarding
│   │   │   ├── dashboard/       # Sales Overview & Analytics
│   │   │   ├── products/        # Product CRUD + MinIO Upload
│   │   │   ├── categories/      # Category CRUD + Invalidation
│   │   │   └── roles/           # PBAC Permission Checkboxes
│   ├── (storefront)/            # White-Labeled Storefront Routes
│   │   ├── page.tsx             # Catalog / Product Grid
│   │   ├── products/[id]/       # Product Details & Image Viewer
│   │   ├── cart/                # Slide-over Drawer / Checkout
│   │   ├── orders/              # Customer Order History (/my-orders)
│   │   └── auth/                # Customer Login / Register
```

---

## 3. API Client & Interceptor Specification

### Base API Configuration
* **Base URL:** `http://localhost:5131/api/v2` (configurable via `NEXT_PUBLIC_API_URL`)

### Axios Interceptor Rules:
1. **Request Interceptor:**
   * Attach `Authorization: Bearer <token>` if a token exists in `useAuthStore`.
   * Attach `X-Tenant-Id: <tenantId>` if a store is active in `useTenantStore`.
2. **Response Interceptor:**
   * Auto-extract `response.data.data` from the backend `ApiResponse<T>` wrapper.
   * If response is `401 Unauthorized`, trigger logout and redirect to the appropriate login page.
   * If response is `403 Forbidden`, show a Toast: *"You do not have permission for this action."*

---

## 4. State Management (Zustand Stores)

### Store 1: `useAuthStore`
```typescript
interface AuthState {
  token: string | null;
  user: {
    userId: string;
    email: string;
    fullName: string;
    role: string;
    tenantId?: string;
    permissions: string[];
  } | null;
  setAuth: (token: string, user: any) => void;
  logout: () => void;
  hasPermission: (permissionCode: string) => boolean;
}
```

### Store 2: `useTenantStore` (For Merchants & Store Selection)
```typescript
interface TenantState {
  activeTenant: {
    tenantId: string;
    storeName: string;
    subdomain: string;
    customDomain?: string;
  } | null;
  availableStores: Array<{
    tenantId: string;
    storeName: string;
    subdomain: string;
  }>;
  setActiveTenant: (tenant: any) => void;
  setAvailableStores: (stores: any[]) => void;
}
```

### Store 3: `useCartStore` (Persisted in LocalStorage)
```typescript
interface CartItem {
  productId: string;
  name: string;
  price: number;
  imageUrl?: string;
  quantity: number;
  stockQuantity: number;
}

interface CartState {
  items: CartItem[];
  addItem: (product: CartItem) => void;
  removeItem: (productId: string) => void;
  updateQuantity: (productId: string, quantity: number) => void;
  clearCart: () => void;
  totalAmount: () => number;
  totalItems: () => number;
}
```

---

## 5. Detailed Screen & Component Specifications

---

### MODULE A: White-Labeled Customer Storefront

#### 1. Storefront Home & Catalog (`/`)
* **Header / Navigation:**
  * Store Name / Logo (from active tenant).
  * Search bar with 300ms debounce (queries `?searchValue=`).
  * Customer Account button (`Login` / `My Orders` / `Logout`).
  * Cart Icon with live badge count (`useCartStore.totalItems()`).
* **Category Pill Bar:**
  * Horizontal scrollable chips (`All`, `Electronics`, `Footwear`, etc.).
  * Clicking filters the product query by `categoryId`.
* **Product Grid:**
  * Responsive 4-column layout (`grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4`).
  * **Product Card Components:**
    * Image container with fallback placeholder (renders MinIO S3 `imageUrl`).
    * Product Name & Category badge.
    * Price formatted cleanly (e.g. `$299.99`).
    * Stock indicator:
      * Green: In Stock (`StockQuantity > 5`)
      * Orange: Low Stock (`StockQuantity <= 5`)
      * Red / Disabled: Out of Stock (`StockQuantity === 0`)
    * **"Add to Cart"** quick-button with optimistic feedback.
* **Pagination:**
  * Next / Prev controls utilizing the backend `PaginatedResult<ProductReadDto>`.

#### 2. Cart Drawer & One-Click Checkout (`Slide-over Sheet`)
* Slides out from the right when clicking the cart icon.
* **Item Rows:** Image thumbnail, title, price, quantity stepper (`+` / `-`), remove button.
* **Order Summary:** Subtotal, Tax/Shipping (if applicable), and Total Amount.
* **Checkout Button:**
  * If customer is **NOT logged in**: Opens Customer Login / Register modal first.
  * If customer is **logged in**:
    * Calls `POST /api/v2/orders/checkout` with `{ items: [{ productId, quantity }] }`.
    * Disables button and displays a spinner.
    * On `201 Created`: Shows a success modal with order invoice ID, clears the cart, and routes to `/orders/[orderId]`.
    * On `400 Bad Request` (e.g. stock exceeded): Displays an inline error alert highlighting which product has insufficient inventory.

#### 3. Customer Order History (`/orders/my-orders`)
* Calls `GET /api/v2/orders/my-orders`.
* Table/Card list of all past orders for this customer:
  * Order ID, Date, Status badge (`Pending` in yellow, `Completed` in green, `Cancelled` in red).
  * Expandable accordion showing purchased items (Product Name, Unit Price snapshot, Quantity, Subtotal).

---

### MODULE B: Merchant Admin Portal

#### 1. Merchant Authentication & Store Switcher (`/admin/login` & `/admin/stores`)
* **Login Form:** Email & Password (`POST /api/v2/auth/login`).
* **Store Switcher Hub (`/admin/stores`):**
  * Calls `GET /api/v2/tenants/my-stores`.
  * Grid of store cards displaying:
    * Store Name, Subdomain (`subdomain.byvstore.com`), Status (`Active`).
    * **"Manage Store"** button: Sets active store in `useTenantStore`, updates `X-Tenant-Id`, and navigates to the admin dashboard.
  * **"Create New Store"** button:
    * Modal with fields: `Store Name`, `Subdomain`, `Custom Domain` (optional).
    * Calls `POST /api/v2/tenants`.

#### 2. Admin Dashboard Overview (`/admin/dashboard`)
* Calls `GET /api/v2/analytics/sales-overview`.
* **Top Metric Cards (KPIs):**
  1. 💰 Total Revenue (`TotalRevenue`)
  2. 📦 Total Orders (`TotalOrdersCount`)
  3. 👥 Total Customers (`TotalCustomersCount`)
  4. ⚠️ Low Stock Alert (`LowStockProductsCount`)
* **Top Selling Products Table:**
  * Calls `GET /api/v2/analytics/top-selling-products?count=5`.
  * Displays Product Name, Units Sold, and Total Revenue Generated.

#### 3. Product Inventory & MinIO Image Upload (`/admin/products`)
* Calls `GET /api/v2/products` with search, sort, and pagination.
* **Action: "Add New Product" Modal:**
  * Fields: `Name`, `Price`, `StockQuantity`, `CategoryId` (dropdown fetched from `/categories`).
  * **Drag & Drop Image Upload Component:**
    * Accepts `.jpg`, `.png`, `.webp` (Max 5MB).
    * Shows instant local image preview before submitting.
    * Submits as `multipart/form-data` using `FormData` directly to `POST /api/v2/products`.
* **Product Table Columns:** Thumbnail (loaded from MinIO S3), Name, Category, Price, Stock, Actions (`Edit`, `Soft Delete`).
* **Soft Delete Action:**
  * Calls `DELETE /api/v2/products/{id}` with confirmation prompt.

#### 4. Category Management & Cache Invalidation (`/admin/categories`)
* Calls `GET /api/v2/categories`.
* List of categories with `Add`, `Edit`, and `Delete` dialogs.
* Shows an alert notice: *"Updates will automatically purge the Redis cache."*

#### 5. Dynamic PBAC Role & Permission Manager (`/admin/roles`)
* Visualizes the Discord-style permission management built in the backend:
* Calls `GET /api/v2/roles/permissions` (gets all available permissions).
* Calls `GET /api/v2/roles` (gets all roles with their assigned permissions).
* **"Create Custom Role" Drawer:**
  * Role Name (e.g. `InventoryManager`), Description.
  * **Permission Checkbox Grid:**
    * Categories: `[ ] Create` `[ ] Update` `[ ] Delete`
    * Products: `[ ] Create` `[ ] Update` `[ ] Delete`
    * Orders: `[ ] Create` `[ ] Update Status`
    * Analytics: `[ ] View Dashboards`
  * Calls `POST /api/v2/roles` with `{ name, description, permissionIds: [...] }`.

---

## 6. Complete API Mapping Matrix

| Page / Feature | HTTP Method | Backend API Endpoint | Request Body / Params | Expected Response |
| :--- | :---: | :--- | :--- | :--- |
| **Merchant Login** | `POST` | `/auth/login` | `{ email, password }` | `{ token, user }` |
| **Merchant Stores**| `GET` | `/tenants/my-stores` | — | `List<TenantReadDto>` |
| **Create Store** | `POST` | `/tenants` | `{ storeName, subdomain, ownerEmail, ... }` | `TenantReadDto` |
| **Storefront Catalog** | `GET` | `/products` | `?pageNumber=&pageSize=&searchValue=&sortOrder=` | `PaginatedResult<ProductReadDto>` |
| **Categories Bar** | `GET` | `/categories` | `?pageNumber=1&pageSize=50` | `PaginatedResult<CategoryReadDto>` |
| **Customer Register**| `POST` | `/auth/register` | `{ email, fullName, password, role: 1 }` | `{ token, user }` |
| **Customer Checkout**| `POST` | `/orders/checkout` | `{ items: [{ productId, quantity }] }` | `OrderReadDto` (201 Created) |
| **Customer Orders** | `GET` | `/orders/my-orders` | — | `List<OrderReadDto>` |
| **Admin KPIs** | `GET` | `/analytics/sales-overview` | — | `SalesOverviewDto` |
| **Admin Top Products**| `GET` | `/analytics/top-selling-products` | `?count=5` | `List<TopSellingProductDto>` |
| **Upload Product** | `POST` | `/products` | `multipart/form-data` (`Name, Price, Stock, CategoryId, Image`) | `ProductReadDto` (Contains `imageUrl`) |
| **Roles & PBAC** | `GET` | `/roles/permissions` | — | `List<PermissionReadDto>` |
| **Create Role** | `POST` | `/roles` | `{ name, description, permissionIds: [] }` | `RoleReadDto` |

---

## 7. Environment Variables (`.env.local`)

```env
# Backend ASP.NET Core API Base URL
NEXT_PUBLIC_API_URL=http://localhost:5131/api/v2

# Base Domain for Multi-Tenancy (used in production or local /etc/hosts testing)
NEXT_PUBLIC_ROOT_DOMAIN=byvstore.com

# MinIO Public Media URL (For rendering product images directly in browser)
NEXT_PUBLIC_MEDIA_URL=http://localhost:9000
```

---