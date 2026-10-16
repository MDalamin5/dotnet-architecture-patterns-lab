import axios, { type AxiosError, type InternalAxiosRequestConfig } from "axios";
import { toast } from "sonner";
import { env } from "@/lib/env";
import type { ApiResponse } from "@/types/api";
import { useTenantStore } from "@/stores/tenant";
import { currentSession } from "@/stores/session";

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
  // Merchant (/admin) and customer (/store) sessions are separate; pick by where the browser is.
  const { auth, tenantId } = currentSession();
  // An explicit Authorization header (e.g. /auth/me right after login) wins over the stored session.
  if (auth.token && !config.headers.has("Authorization")) config.headers.set("Authorization", `Bearer ${auth.token}`);
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
      const { portal, auth } = currentSession();
      const hadSession = !!auth.token;
      auth.logout();

      if (portal === "admin") {
        useTenantStore.getState().reset();
        // Hard navigation on purpose: outside React, and it drops all in-memory query state.
        // eslint-disable-next-line @next/next/no-location-assign-relative-destination
        if (!window.location.pathname.startsWith("/admin/login")) window.location.assign("/admin/login");
      } else if (hadSession) {
        toast.error("Your session has expired. Please sign in again.");
      }
    }

    if (status === 403) toast.error("You do not have permission for this action.");

    return Promise.reject(error);
  },
);
