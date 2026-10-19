import { create } from "zustand";
import { persist } from "zustand/middleware";

/** OrderItemCheckoutDto allows 1-100 units per line. */
export const MAX_LINE_QUANTITY = 100;

export interface CartItem {
  productId: string;
  name: string;
  price: number;
  imageUrl?: string | null;
  quantity: number;
  stockQuantity: number;
}

interface CartState {
  /** The store this cart belongs to — a cart is never shared across tenants. */
  tenantId: string | null;
  items: CartItem[];
  bindTenant: (tenantId: string) => void;
  addItem: (product: CartItem) => void;
  removeItem: (productId: string) => void;
  updateQuantity: (productId: string, quantity: number) => void;
  clearCart: () => void;
  totalAmount: () => number;
  totalItems: () => number;
}

const maxFor = (item: Pick<CartItem, "stockQuantity">) => Math.max(0, Math.min(item.stockQuantity, MAX_LINE_QUANTITY));

export const useCartStore = create<CartState>()(
  persist(
    (set, get) => ({
      tenantId: null,
      items: [],

      bindTenant: (tenantId) => {
        if (get().tenantId !== tenantId) set({ tenantId, items: [] });
      },

      addItem: (product) =>
        set((state) => {
          const limit = maxFor(product);
          if (limit === 0) return state;
          const existing = state.items.find((i) => i.productId === product.productId);
          if (!existing) {
            return { items: [...state.items, { ...product, quantity: Math.min(product.quantity, limit) }] };
          }
          return {
            items: state.items.map((i) =>
              i.productId === product.productId
                ? { ...i, ...product, quantity: Math.min(i.quantity + product.quantity, limit) }
                : i,
            ),
          };
        }),

      removeItem: (productId) => set((state) => ({ items: state.items.filter((i) => i.productId !== productId) })),

      updateQuantity: (productId, quantity) =>
        set((state) => ({
          items:
            quantity <= 0
              ? state.items.filter((i) => i.productId !== productId)
              : state.items.map((i) =>
                  i.productId === productId ? { ...i, quantity: Math.min(quantity, maxFor(i)) } : i,
                ),
        })),

      clearCart: () => set({ items: [] }),
      totalAmount: () => get().items.reduce((sum, i) => sum + i.price * i.quantity, 0),
      totalItems: () => get().items.reduce((sum, i) => sum + i.quantity, 0),
    }),
    {
      name: "byv-cart",
      partialize: (state) => ({ tenantId: state.tenantId, items: state.items }),
      skipHydration: true,
    },
  ),
);
