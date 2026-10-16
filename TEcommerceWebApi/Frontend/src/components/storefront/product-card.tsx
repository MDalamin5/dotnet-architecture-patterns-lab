"use client";

import { useEffect, useRef, useState } from "react";
import { Check, ShoppingCart } from "lucide-react";
import { toast } from "sonner";
import { formatCurrency } from "@/lib/format";
import { useCartStore } from "@/stores/cart";
import type { ProductReadDto } from "@/types/api";
import { ProductImage } from "@/components/common/product-image";
import { StockBadge } from "@/components/common/status-badges";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter } from "@/components/ui/card";

export function ProductCard({ product }: { product: ProductReadDto }) {
  const addItem = useCartStore((s) => s.addItem);
  const inCart = useCartStore((s) => s.items.find((i) => i.productId === product.productId)?.quantity ?? 0);
  const [justAdded, setJustAdded] = useState(false);
  const timer = useRef<ReturnType<typeof setTimeout>>(undefined);
  useEffect(() => () => clearTimeout(timer.current), []);

  const outOfStock = product.stockQuantity <= 0;
  const maxedOut = inCart >= product.stockQuantity;

  function add() {
    if (maxedOut) {
      toast.info(`You already have all ${product.stockQuantity} available units in your cart.`);
      return;
    }
    addItem({
      productId: product.productId,
      name: product.name,
      price: product.price,
      imageUrl: product.imageUrl,
      stockQuantity: product.stockQuantity,
      quantity: 1,
    });
    // Optimistic feedback — the cart is local, so this is instant.
    setJustAdded(true);
    clearTimeout(timer.current);
    timer.current = setTimeout(() => setJustAdded(false), 1200);
  }

  return (
    <Card className="gap-0 overflow-hidden py-0">
      <ProductImage src={product.imageUrl} alt={product.name} className="aspect-square w-full" />
      <CardContent className="space-y-2 p-4">
        <div className="flex items-start justify-between gap-2">
          <h3 className="line-clamp-2 font-medium leading-snug">{product.name}</h3>
        </div>
        <Badge variant="secondary">{product.categoryName}</Badge>
        <div className="flex items-center justify-between gap-2 pt-1">
          <p className="text-lg font-semibold tabular-nums">{formatCurrency(product.price)}</p>
          <StockBadge quantity={product.stockQuantity} />
        </div>
      </CardContent>
      <CardFooter className="p-4 pt-0">
        <Button className="w-full" onClick={add} disabled={outOfStock} variant={justAdded ? "secondary" : "default"}>
          {outOfStock ? (
            "Out of stock"
          ) : justAdded ? (
            <>
              <Check /> Added
            </>
          ) : (
            <>
              <ShoppingCart /> Add to cart
            </>
          )}
        </Button>
      </CardFooter>
    </Card>
  );
}
