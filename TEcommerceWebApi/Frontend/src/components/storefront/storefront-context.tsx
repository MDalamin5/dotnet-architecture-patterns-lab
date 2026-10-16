"use client";

import { createContext, useContext } from "react";
import type { ActiveTenant } from "@/stores/tenant";

interface StorefrontContextValue {
  /** Path prefix for in-app links, e.g. /store/<tenantId>. */
  base: string;
  tenant: ActiveTenant;
}

export const StorefrontContext = createContext<StorefrontContextValue | null>(null);

export function useStorefront(): StorefrontContextValue {
  const ctx = useContext(StorefrontContext);
  if (!ctx) throw new Error("useStorefront must be used inside <StorefrontShell>");
  return ctx;
}
