"use client";

import { LogIn } from "lucide-react";
import { useAuthStore } from "@/stores/auth";
import { useStorefrontUi } from "@/stores/ui";
import { EmptyState } from "@/components/common/empty-state";
import { Button } from "@/components/ui/button";

/** Renders children only for a signed-in customer; otherwise prompts for login. */
export function RequireLogin({ children }: { children: React.ReactNode }) {
  const token = useAuthStore((s) => s.token);
  const openAuth = useStorefrontUi((s) => s.openAuth);

  if (token) return <>{children}</>;
  return (
    <EmptyState
      icon={LogIn}
      title="Sign in to view your orders"
      description="Your order history is tied to your account."
      action={<Button onClick={() => openAuth("login")}>Sign in</Button>}
    />
  );
}
