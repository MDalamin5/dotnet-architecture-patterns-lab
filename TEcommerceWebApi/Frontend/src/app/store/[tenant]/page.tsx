"use client";

import { useMemo } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { PackageSearch } from "lucide-react";
import { useCategories, useProducts } from "@/hooks/use-catalog";
import { CategoryPills } from "@/components/storefront/category-pills";
import { ProductCard } from "@/components/storefront/product-card";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorAlert } from "@/components/common/error-alert";
import { Pagination } from "@/components/common/pagination";
import { Skeleton } from "@/components/ui/skeleton";

/** Backend caps pageSize at 6 (QueryParameters.MAX_PAGE_SIZE). */
const PAGE_SIZE = 6;

const gridClass = "grid grid-cols-1 gap-4 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4";

export default function StorefrontCatalogPage() {
  const router = useRouter();
  const pathname = usePathname();
  const params = useSearchParams();

  const query = params.get("q")?.trim() ?? "";
  const categoryId = params.get("category");
  const page = Math.max(1, Number(params.get("page")) || 1);

  const categories = useCategories();
  const category = categories.data?.find((c) => c.categoryId === categoryId) ?? null;

  // GET /products has no categoryId filter. `searchValue` matches product OR category name, so a
  // category pill searches by its name and we drop any false positives (same-name products from
  // other categories) client-side.
  const searchValue = query || category?.name || undefined;
  const waitingForCategory = !!categoryId && !query && categories.isPending;

  const products = useProducts({ pageNumber: page, pageSize: PAGE_SIZE, searchValue, sortOrder: "CreatedAtDesc" });

  const items = useMemo(() => {
    const all = products.data?.items ?? [];
    return category && !query ? all.filter((p) => p.categoryId === category.categoryId) : all;
  }, [products.data, category, query]);

  function navigate(next: { category?: string | null; page?: number }) {
    const sp = new URLSearchParams();
    if (next.category) sp.set("category", next.category);
    else if (query) sp.set("q", query);
    if (next.page && next.page > 1) sp.set("page", String(next.page));
    const qs = sp.toString();
    router.push(qs ? `${pathname}?${qs}` : pathname);
  }

  const loading = products.isPending || waitingForCategory;

  return (
    <div className="space-y-6">
      <CategoryPills
        categories={categories.data}
        loading={categories.isPending}
        selectedId={category?.categoryId ?? null}
        onSelect={(id) => navigate({ category: id })}
      />

      {query && (
        <p className="text-sm text-muted-foreground">
          Results for <span className="font-medium text-foreground">“{query}”</span>
        </p>
      )}

      <ErrorAlert error={products.error} title="Couldn't load products" />

      {loading ? (
        <div className={gridClass}>
          {Array.from({ length: PAGE_SIZE }, (_, i) => (
            <Skeleton key={i} className="aspect-[3/4] rounded-xl" />
          ))}
        </div>
      ) : items.length === 0 && !products.isError ? (
        <EmptyState
          icon={PackageSearch}
          title="No products found"
          description={query ? "Try a different search term." : "This store has no products here yet."}
        />
      ) : (
        <div className={gridClass}>
          {items.map((p) => (
            <ProductCard key={p.productId} product={p} />
          ))}
        </div>
      )}

      {products.data && (
        <Pagination
          page={products.data.pageNumber}
          totalPages={products.data.totalPages}
          totalCount={products.data.totalCount}
          disabled={products.isFetching}
          onPageChange={(p) => navigate({ category: category?.categoryId ?? null, page: p })}
        />
      )}
    </div>
  );
}
