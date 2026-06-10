import type { AxiosRequestConfig } from 'axios';
import { apiClient } from './apiClient';

// Used by orval-generated hooks as the underlying HTTP transport.
// Routes all generated API calls through apiClient so JWT interceptor
// and the 401→refresh logic are always active.
export const orvalMutator = <T>(config: AxiosRequestConfig): Promise<T> =>
  apiClient.request<T>(config).then((r) => r.data);
