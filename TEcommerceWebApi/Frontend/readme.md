# BYV Store — Frontend

Multi-tenant e-commerce frontend for the `TEcommerceWebApi` backend: a white-labeled **customer storefront** and a **merchant admin portal**.

**Stack:** Next.js 16 (App Router) · React 19 · TypeScript · Tailwind CSS v4 · shadcn/ui (Base UI) · TanStack Query v5 · Axios · Zustand · React Hook Form + Zod.

> The spec lives in [`SPECS/prd.md`](SPECS/prd.md). Where the backend contradicts it, the code follows the backend — see [Backend notes](#backend-notes).

## Run it

Prerequisites: Node 20+, and the API running on `http://localhost:5131` (PostgreSQL required; Redis and MinIO are optional for browsing but MinIO is required for product image uploads).

```bash
cp .env.local.example .env.local
npm install --legacy-peer-deps   # @hookform/resolvers declares an optional zod@3 peer
npm run dev                      # http://localhost:3000
```

| Script | |
| --- | --- |
| `npm run dev` | dev server |
| `npm run build` / `npm start` | production build / serve |
| `npm run lint` | ESLint |

### Environment

| Variable | Default | Purpose |
| --- | --- | --- |
| `NEXT_PUBLIC_API_URL` | `/api/v2` | Axios base URL. A relative path goes through the Next.js rewrite proxy (below). |
| `API_PROXY_TARGET` | `http://localhost:5131` | Server-side target of the `/api/v2/*` rewrite (`next.config.ts`). |
| `NEXT_PUBLIC_ROOT_DOMAIN` | `byvstore.com` | Base domain for `<subdomain>.byvstore.com` routing and display. |
| `NEXT_PUBLIC_MEDIA_URL` | `http://localhost:9000` | Prefix for *relative* image paths (the API normally returns absolute MinIO URLs). |

**Why a proxy?** The ASP.NET API has no CORS policy, so a browser on `:3000` can't call `:5131` directly. `next.config.ts` rewrites `/api/v2/*` to the API, making calls same-origin. If you later add CORS to the backend you can set `NEXT_PUBLIC_API_URL=http://localhost:5131/api/v2`.

## Try it

1. Create your first store. The admin login has no sign-up form (store creation is a public API endpoint, and it also creates the owner account), so seed it once:
   ```bash
   curl -X POST http://localhost:5131/api/v2/tenants -H 'Content-Type: application/json' \
     -d '{"storeName":"Demo","subdomain":"demo","ownerEmail":"me@example.com","ownerFullName":"Me","ownerPassword":"secret123"}'
   ```
   (After that, additional stores can be created from **/admin/stores → Create new store**.)
2. Sign in at **/admin/login** → pick the store → create a category, then products.
3. Open the storefront from the store card's *external link* button: **/store/\<tenantId\>**. Register a customer, add to cart, check out, view **My orders**.

## Routes

| Route | What |
| --- | --- |
| `/` | Platform landing |
| `/admin/login` | Merchant sign-in (role `Admin` only) |
| `/admin/stores` | Store switcher hub + create store |
| `/admin/dashboard` | KPIs + top-selling products |
| `/admin/products` | Product CRUD, search/sort/pagination, drag-and-drop image upload (`multipart/form-data`) |
| `/admin/categories` | Category CRUD |
| `/admin/roles` | PBAC: create roles, edit permissions with a grouped checkbox grid |
| `/store/[tenant]` | Storefront catalog (search, category pills, pagination) |
| `/store/[tenant]/orders/my-orders` | Customer order history |
| `/store/[tenant]/orders/[orderId]` | Single order |

`[tenant]` is a tenant GUID locally. `src/proxy.ts` (Next 16's replacement for `middleware.ts`) rewrites `nike.byvstore.com/*` → `/store/nike/*`; `app.`/`www.` hosts pass through to the platform routes.

## Architecture

```
src/
├─ app/                    routes (admin/ = merchant portal, store/[tenant]/ = storefront)
├─ components/
│  ├─ ui/                  shadcn/ui primitives (Button, Card, Dialog, Sheet, Table, Input, Badge, DropdownMenu, …)
│  ├─ admin/  storefront/  feature components
│  └─ common/              shared: ProductImage, Pagination, ErrorAlert, FormField, ConfirmDialog, badges
├─ hooks/                  TanStack Query hooks (use-catalog, use-orders, use-admin) + auth mutations
├─ lib/
│  ├─ api/                 Axios client (+ interceptors) and one module per backend controller
│  ├─ errors.ts format.ts  error-message extraction, currency/date/media helpers
├─ stores/                 Zustand stores (see below)
├─ types/api.ts            TypeScript mirrors of the backend DTOs
└─ proxy.ts                host-based tenant routing
```

### API client (`src/lib/api/client.ts`)
* **Request:** adds `Authorization: Bearer <token>` and `X-Tenant-Id` (skippable per request via `skipTenant`).
* **Response:** unwraps `ApiResponse<T>` → `response.data` is `T`. `401` → logout (admin: back to `/admin/login`; storefront: toast). `403` → toast *"You do not have permission for this action."*
* Errors from both shapes the backend emits (`ApiResponse.errors[]` and ASP.NET `ProblemDetails`) are normalised by `getErrorMessages`.

### State (Zustand, persisted to localStorage)
| Store | Holds |
| --- | --- |
| `useAuthStore` | **Customer** session: token, user, `hasPermission()` |
| `useMerchantAuthStore` | **Merchant** session (same shape) used by `/admin` |
| `useTenantStore` | Merchant's `activeTenant` + `availableStores` |
| `useStorefrontTenantStore` | The store a customer is browsing |
| `useCartStore` | Cart items, bound to a tenant (cleared when you switch stores) |

The spec defines one auth/tenant store each; they are split per portal because on `localhost` both portals share an origin, and a customer login in one tab would otherwise overwrite the merchant session in another. The interceptor picks the session from the URL (`/admin…` → merchant). All persisted stores use `skipHydration` and are rehydrated after mount (`stores/hydration.ts`) to avoid SSR hydration mismatches.

## Backend notes

Things discovered while integrating that you should know about:

1. **No CORS** → worked around with the rewrite proxy.
2. **`GET /products` ignores `categoryId`.** Category pills search by category name (`searchValue` also matches category names) and drop false positives client-side. A real `categoryId` filter in `QueryParameters` would be cleaner. Search and category filter are therefore mutually exclusive in the storefront.
3. **Page size is capped at 6** (`QueryParameters.MAX_PAGE_SIZE`), also for `/categories`; the frontend walks all pages to build the category list.
4. **`GetAllProducts` didn't project `ImageUrl`/`StockQuantity`** (list returned `null`/`0`, so every product looked out of stock). Fixed in `Services/ProductService.cs`.
5. **No public tenant lookup.** Storefront URLs use the tenant GUID (`/store/<id>`), and the header shows a generic name unless the visitor is the merchant. Subdomain routing (`nike.byvstore.com`) calls `GET /tenants/by-subdomain/{subdomain}`, which **does not exist yet** — add it (returning `TenantReadDto`) to make subdomain storefronts work.
6. **`PUT`/`DELETE /products/{id}` have no `[Authorize]`**, so anyone with a tenant id can edit or delete products. The UI doesn't gate them by permission for that reason; the backend should.
7. **No `*.update` permissions are seeded** (`categories.update`, `products.update` exist as policies but not as rows), so the seeded `Admin` can't edit categories (the Edit button is disabled with an explanatory tooltip). Create the permissions or assign them in `/admin/roles` once they exist.
8. **Product image can't be changed after creation** (`PUT` is JSON only), so the edit dialog doesn't offer an upload.
9. `POST /tenants` always creates a new Admin user; use your own login email as `ownerEmail` so the store appears under *your stores* (`/tenants/my-stores` matches on owner email).

## Not verified

* Image upload/rendering against a real **MinIO** (not running in the test environment — the multipart request reaches the API and fails only with `Connection refused localhost:9000`).
* Subdomain routing (needs backend note 5 and DNS/hosts entries).
