"use client";

import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";
import type { CategoryReadDto } from "@/types/api";

interface CategoryPillsProps {
  categories: CategoryReadDto[] | undefined;
  loading: boolean;
  selectedId: string | null;
  onSelect: (categoryId: string | null) => void;
}

const pill = "shrink-0 rounded-full border px-4 py-1.5 text-sm font-medium transition-colors";

export function CategoryPills({ categories, loading, selectedId, onSelect }: CategoryPillsProps) {
  if (loading) {
    return (
      <div className="flex gap-2">
        {Array.from({ length: 5 }, (_, i) => (
          <Skeleton key={i} className="h-8 w-24 rounded-full" />
        ))}
      </div>
    );
  }

  return (
    <div role="tablist" aria-label="Product categories" className="-mx-4 flex gap-2 overflow-x-auto px-4 pb-1 sm:mx-0 sm:px-0">
      <button
        role="tab"
        aria-selected={selectedId === null}
        onClick={() => onSelect(null)}
        className={cn(pill, selectedId === null ? "border-primary bg-primary text-primary-foreground" : "hover:bg-muted")}
      >
        All
      </button>
      {categories?.map((c) => (
        <button
          key={c.categoryId}
          role="tab"
          aria-selected={selectedId === c.categoryId}
          onClick={() => onSelect(c.categoryId)}
          className={cn(
            pill,
            selectedId === c.categoryId ? "border-primary bg-primary text-primary-foreground" : "hover:bg-muted",
          )}
        >
          {c.name}
        </button>
      ))}
    </div>
  );
}
