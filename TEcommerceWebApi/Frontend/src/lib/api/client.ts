import axios, { type AxiosError, type InternalAxiosRequestConfig } from "axios";
import { toast } from "sonner";
import { env } from "@/lib/env";
import type { ApiResponse } from "@/types/api";
import { useAuthStore } from "@/stores/auth";
import { useTenantStore } from "@/stores/tenant";

declare module "axios" {
  interface AxiosRequestConfig {
    /** Do not send X-Tenant-Id (merchant "global" login / store listing). */
    skipTenant?: boolean;
    /** Do not run the global 401 handler (a 401 from a login form just means bad credentials). */
    skipAuthRedirect?: boolean;
  }
}

export const http = axios.create({
  baseURL: env.apiUrl,
  headers: { Accept: "application/json" },
});

const isApiEnvelope = (body: unknown): body is ApiResponse<unknown> =>
  typeof body === "object" && body !== null && "success" in body && "statusCode" in body;

// ---- Request: Authorization + X-Tenant-Id ------------------------------------------------
http.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = useAuthStore.getState().token;
  if (token) config.headers.set("Authorization", `Bearer ${token}`);

  const tenantId = useTenantStore.getState().activeTenant?.tenantId;
  if (tenantId && !config.skipTenant) config.headers.set("X-Tenant-Id", tenantId);

  // Let the browser set the multipart boundary itself.
  if (typeof FormData !== "undefined" && config.data instanceof FormData) {
    config.headers.delete("Content-Type");
  }
  return config;
});

// ---- Response: unwrap ApiResponse<T>, handle 401 / 403 -----------------------------------
http.interceptors.response.use(
  (response) => {
    if (isApiEnvelope(response.data)) response.data = response.data.data;
    return response;
  },
  (error: AxiosError) => {
    const status = error.response?.status;

    if (status === 401 && !error.config?.skipAuthRedirect && typeof window !== "undefined") {
      const hadSession = !!useAuthStore.getState().token;
      useAuthStore.getState().logout();

      if (window.location.pathname.startsWith("/admin")) {
        useTenantStore.getState().reset();
        if (!window.location.pathname.startsWith("/admin/login")) window.location.assign("/admin/login");
      } else if (hadSession) {
        toast.error("Your session has expired. Please sign in again.");
      }
    }

    if (status === 403) toast.error("You do not have permission for this action.");

    return Promise.reject(error);
  },
);
