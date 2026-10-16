import axios from "axios";

/**
 * Pulls human-readable messages out of an API failure. Handles both shapes the backend emits:
 *  - ApiResponse<T>.errors: string[]  (controllers / GlobalExceptionMiddleware)
 *  - ASP.NET ProblemDetails.errors: Record<field, string[]>  (automatic model-validation 400s)
 */
export function getErrorMessages(error: unknown, fallback = "Something went wrong. Please try again."): string[] {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as
      | { errors?: string[] | Record<string, string[]> | null; message?: string; title?: string }
      | undefined;

    if (Array.isArray(data?.errors) && data.errors.length) return data.errors;
    if (data?.errors && typeof data.errors === "object") {
      const flat = Object.values(data.errors).flat();
      if (flat.length) return flat;
    }
    if (data?.message) return [data.message];
    if (data?.title) return [data.title];
    if (!error.response) return ["Cannot reach the server. Is the API running?"];
  }
  if (error instanceof Error && error.message) return [error.message];
  return [fallback];
}

export function getErrorMessage(error: unknown, fallback?: string): string {
  return getErrorMessages(error, fallback).join(" ");
}

export function getErrorStatus(error: unknown): number | undefined {
  return axios.isAxiosError(error) ? error.response?.status : undefined;
}
