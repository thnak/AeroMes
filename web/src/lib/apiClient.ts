import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api',
  headers: { 'Content-Type': 'application/json' },
  withCredentials: true, // sends cookies for cookie-based auth (Web)
});

// ─── Request: attach Bearer token for API/PDA calls ──────────────────────────
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// ─── Response: 401 → attempt silent refresh ──────────────────────────────────
let refreshPromise: Promise<string> | null = null;

apiClient.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    if (error.response?.status !== 401 || original._retry) {
      return Promise.reject(error);
    }
    original._retry = true;

    if (!refreshPromise) {
      refreshPromise = apiClient
        .post<{ accessToken: string }>('/auth/refresh', {}, { _retry: true } as never)
        .then((r) => {
          const token = r.data.accessToken;
          localStorage.setItem('accessToken', token);
          return token;
        })
        .finally(() => { refreshPromise = null; });
    }

    try {
      const token = await refreshPromise;
      original.headers.Authorization = `Bearer ${token}`;
      return apiClient(original);
    } catch {
      localStorage.removeItem('accessToken');
      window.location.href = '/auth/login';
      return Promise.reject(error);
    }
  },
);

// Typed helper wrappers consumed by React Query hooks
export const api = {
  get: <T>(url: string, params?: Record<string, unknown>) =>
    apiClient.get<T>(url, { params }).then((r) => r.data),
  post: <T>(url: string, data?: unknown) =>
    apiClient.post<T>(url, data).then((r) => r.data),
  put: <T>(url: string, data?: unknown) =>
    apiClient.put<T>(url, data).then((r) => r.data),
  patch: <T>(url: string, data?: unknown) =>
    apiClient.patch<T>(url, data).then((r) => r.data),
  delete: <T>(url: string) =>
    apiClient.delete<T>(url).then((r) => r.data),
};
