import { create } from "zustand";
import type { OrderReadDto } from "@/types/api";

/** Transient storefront UI state shared between the header, cart drawer and auth modal (not persisted). */
interface StorefrontUiState {
  cartOpen: boolean;
  authOpen: boolean;
  authMode: "login" | "register";
  placedOrder: OrderReadDto | null;
  setCartOpen: (open: boolean) => void;
  openAuth: (mode?: "login" | "register") => void;
  setAuthOpen: (open: boolean) => void;
  setAuthMode: (mode: "login" | "register") => void;
  setPlacedOrder: (order: OrderReadDto | null) => void;
}

export const useStorefrontUi = create<StorefrontUiState>((set) => ({
  cartOpen: false,
  authOpen: false,
  authMode: "login",
  placedOrder: null,
  setCartOpen: (cartOpen) => set({ cartOpen }),
  openAuth: (authMode = "login") => set({ authOpen: true, authMode }),
  setAuthOpen: (authOpen) => set({ authOpen }),
  setAuthMode: (authMode) => set({ authMode }),
  setPlacedOrder: (placedOrder) => set({ placedOrder }),
}));
