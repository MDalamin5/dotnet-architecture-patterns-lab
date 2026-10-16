"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { useLogin, useRegister } from "@/hooks/use-auth";
import { useStorefrontUi } from "@/stores/ui";
import { ErrorAlert } from "@/components/common/error-alert";
import { FormField } from "@/components/common/form-field";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";

const loginSchema = z.object({
  email: z.string().email("Enter a valid email"),
  password: z.string().min(1, "Password is required"),
});

const registerSchema = z.object({
  fullName: z.string().min(2, "At least 2 characters").max(100),
  email: z.string().email("Enter a valid email"),
  password: z.string().min(6, "At least 6 characters"),
});

type LoginValues = z.infer<typeof loginSchema>;
type RegisterValues = z.infer<typeof registerSchema>;

function LoginForm({ onDone }: { onDone: () => void }) {
  const login = useLogin({ portal: "store" });
  const form = useForm<LoginValues>({ resolver: zodResolver(loginSchema) });
  const { errors } = form.formState;

  return (
    <form
      className="space-y-4"
      onSubmit={form.handleSubmit((values) =>
        login.mutate(values, {
          onSuccess: (user) => {
            toast.success(`Welcome back, ${user.fullName}!`);
            onDone();
          },
        }),
      )}
    >
      <FormField label="Email" error={errors.email?.message}>
        <Input type="email" autoComplete="email" {...form.register("email")} />
      </FormField>
      <FormField label="Password" error={errors.password?.message}>
        <Input type="password" autoComplete="current-password" {...form.register("password")} />
      </FormField>
      <ErrorAlert error={login.error} title="Sign in failed" />
      <Button type="submit" className="w-full" disabled={login.isPending}>
        {login.isPending ? "Signing in…" : "Sign in"}
      </Button>
    </form>
  );
}

function RegisterForm({ onDone }: { onDone: () => void }) {
  const register = useRegister();
  const form = useForm<RegisterValues>({ resolver: zodResolver(registerSchema) });
  const { errors } = form.formState;

  return (
    <form
      className="space-y-4"
      onSubmit={form.handleSubmit((values) =>
        register.mutate(
          { ...values, role: 1 },
          {
            onSuccess: (user) => {
              toast.success(`Account created. Welcome, ${user.fullName}!`);
              onDone();
            },
          },
        ),
      )}
    >
      <FormField label="Full name" error={errors.fullName?.message}>
        <Input autoComplete="name" {...form.register("fullName")} />
      </FormField>
      <FormField label="Email" error={errors.email?.message}>
        <Input type="email" autoComplete="email" {...form.register("email")} />
      </FormField>
      <FormField label="Password" error={errors.password?.message}>
        <Input type="password" autoComplete="new-password" {...form.register("password")} />
      </FormField>
      <ErrorAlert error={register.error} title="Couldn't create account" />
      <Button type="submit" className="w-full" disabled={register.isPending}>
        {register.isPending ? "Creating account…" : "Create account"}
      </Button>
    </form>
  );
}

export function AuthDialog() {
  const { authOpen, authMode, setAuthOpen, setAuthMode } = useStorefrontUi();
  const isLogin = authMode === "login";

  return (
    <Dialog open={authOpen} onOpenChange={setAuthOpen}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>{isLogin ? "Sign in to your account" : "Create your account"}</DialogTitle>
          <DialogDescription>
            {isLogin ? "Sign in to check out and track your orders." : "It takes a few seconds."}
          </DialogDescription>
        </DialogHeader>

        {/* key resets form state (and any error) when switching tabs */}
        {isLogin ? (
          <LoginForm key="login" onDone={() => setAuthOpen(false)} />
        ) : (
          <RegisterForm key="register" onDone={() => setAuthOpen(false)} />
        )}

        <p className="text-center text-sm text-muted-foreground">
          {isLogin ? "New here? " : "Already have an account? "}
          <button
            type="button"
            className="font-medium text-primary hover:underline"
            onClick={() => setAuthMode(isLogin ? "register" : "login")}
          >
            {isLogin ? "Create an account" : "Sign in"}
          </button>
        </p>
      </DialogContent>
    </Dialog>
  );
}
