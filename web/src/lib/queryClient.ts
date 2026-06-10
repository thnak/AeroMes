import { QueryClient } from '@tanstack/react-query';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,          // 30s — MES data changes, but not per keystroke
      gcTime: 5 * 60_000,         // 5min — keep in cache after unmount
      retry: (failureCount, error) => {
        // Don't retry auth/permission errors
        const status = (error as { response?: { status?: number } })?.response?.status;
        if (status === 401 || status === 403 || status === 404) return false;
        return failureCount < 2;
      },
      refetchOnWindowFocus: false, // production floor — no surprise refetches
    },
    mutations: {
      retry: false,
    },
  },
});
