import { defineConfig } from 'orval';

export default defineConfig({
  aeromes: {
    input: {
      // Use local spec snapshot by default (no server required).
      // Refresh it with: npm run fetch:spec
      // Override with OPENAPI_SPEC env var to point at a live server.
      target: process.env.OPENAPI_SPEC ?? '../aeromes-openapi.json',
    },
    output: {
      // One file per API tag (matches controller names), e.g.:
      //   src/api/products.ts  →  useGetProducts, useCreateProduct, …
      //   src/api/workOrders.ts → useGetWorkOrders, useGetWorkOrderById, …
      mode: 'tags-split',
      target: 'src/api',
      schemas: 'src/api/model',
      client: 'react-query',
      httpClient: 'axios',
      override: {
        // All generated hooks call orvalMutator, which goes through apiClient
        // (JWT interceptor + 401-refresh live here).
        mutator: {
          path: 'src/lib/orvalMutator.ts',
          name: 'orvalMutator',
        },
        query: {
          useQuery: true,
          useInfiniteQuery: false, // enable per-endpoint when needed
          useSuspenseQuery: false,
          // Mirror queryClient defaults — overridable per-hook at the call site
          options: {
            staleTime: 30000,
          },
        },
      },
    },
  },
});
