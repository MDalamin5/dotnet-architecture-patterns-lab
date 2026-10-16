"use client";

import { useEffect, useState } from "react";
import { isAxiosError } from "axios";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Toaster } from "@/components/ui/sonner";
import { rehydrateStores } from "@/stores/hydration";

function makeQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        refetchOnWindowFocus: false,
        // 4xx responses won't fix themselves; only retry network/5xx failures.
        retry: (failureCount, error) =>
          failureCount < 2 && !(isAxiosError(error) && (error.response?.status ?? 500) < 500),
      },
    },
  });
}

export function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(makeQueryClient);

  useEffect(() => {
    void rehydrateStores();
  }, []);

  return (
    <QueryClientProvider client={queryClient}>
      {children}
      <Toaster richColors position="top-right" />
    </QueryClientProvider>
  );
}
