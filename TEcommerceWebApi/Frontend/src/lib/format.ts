import { env } from "@/lib/env";

const currency = new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" });
const compact = new Intl.NumberFormat("en-US");

export const formatCurrency = (value: number) => currency.format(value);
export const formatNumber = (value: number) => compact.format(value);

export function formatDate(iso: string, withTime = false): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return iso;
  return date.toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
    ...(withTime ? { hour: "2-digit", minute: "2-digit" } : {}),
  });
}

/** The backend stores absolute MinIO URLs; tolerate relative object keys too. */
export function resolveMediaUrl(imageUrl: string | null | undefined): string | null {
  if (!imageUrl) return null;
  if (/^https?:\/\//i.test(imageUrl)) return imageUrl;
  return `${env.mediaUrl.replace(/\/$/, "")}/${imageUrl.replace(/^\//, "")}`;
}

/** "orders.manage_status" -> "Manage Status" */
export function humanizePermission(code: string): string {
  const action = code.split(".").slice(1).join(" ") || code;
  return action
    .split(/[._\s]+/)
    .map((w) => w.charAt(0).toUpperCase() + w.slice(1))
    .join(" ");
}

export const isGuid = (value: string) =>
  /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value);
