"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useLogin } from "@/hooks/use-auth";
import { useMerchantAuthStore } from "@/stores/auth";
import { useHydrated } from "@/stores/hydration";
import { ErrorAlert } from "@/components/common/error-alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

const schema = z.object({
  email: z.string().email("Enter a valid email"),
  password: z.string().min(1, "Password is required"),
});
type Values = z.infer<typeof schema>;

export default function AdminLoginPage() {
  const router = useRouter();
  const hydrated = useHydrated();
  const alreadyIn = useMerchantAuthStore((s) => !!s.token && s.user?.role === "Admin");
  const login = useLogin({ portal: "admin" });
  const form = useForm<Values>({ resolver: zodResolver(schema) });
  const { errors } = form.formState;

  useEffect(() => {
    if (hydrated && alreadyIn) router.replace("/admin/stores");
  }, [hydrated, alreadyIn, router]);

  return (
    <main className="flex flex-1 items-center justify-center bg-muted/40 px-4 py-12">
      <Card className="w-full max-w-sm">
        <CardHeader>
          <CardTitle className="text-xl">Merchant sign in</CardTitle>
          <CardDescription>Manage your stores, products and sales.</CardDescription>
        </CardHeader>
        <CardContent>
          <form
            className="space-y-4"
            onSubmit={form.handleSubmit((values) =>
              login.mutate(values, { onSuccess: () => router.replace("/admin/stores") }),
            )}
          >
            <div className="space-y-1.5">
              <Label htmlFor="email">Email</Label>
              <Input id="email" type="email" autoComplete="email" {...form.register("email")} />
              {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="password">Password</Label>
              <Input id="password" type="password" autoComplete="current-password" {...form.register("password")} />
              {errors.password && <p className="text-xs text-destructive">{errors.password.message}</p>}
            </div>
            <ErrorAlert error={login.error} title="Sign in failed" />
            <Button type="submit" className="w-full" disabled={login.isPending}>
              {login.isPending ? "Signing in…" : "Sign in"}
            </Button>
          </form>
        </CardContent>
      </Card>
    </main>
  );
}
