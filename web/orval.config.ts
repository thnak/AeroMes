import { defineConfig } from 'orval';

export default defineConfig({
  aeromes: {
    input: {
      // .NET 10 AddOpenApi() default endpoint.
      // For local dev, API must be running: dotnet run --project src/AeroMes.Api
      target: process.env.VITE_API_BASE_URL
        ? `${process.env.VITE_API_BASE_URL}/openapi/v1.json`
        : 'http://localhost:5170/openapi/v1.json',
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
