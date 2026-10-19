import { RequireTenant } from "@/components/admin/admin-guard";

export default function StoreScopedLayout({ children }: { children: React.ReactNode }) {
  return <RequireTenant>{children}</RequireTenant>;
}
