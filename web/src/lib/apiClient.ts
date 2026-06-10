import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';

export interface ApiError {
  title?: string;
  detail?: string;
  code?: string;
  errors?: string[];
}

export function getErrorMessage(err: unknown, fallback = 'An error occurred'): string {
  if (!err) return fallback;
  const axErr = err as AxiosError<ApiError & { errors?: Record<string, string[]> }>;
  if (axErr.response?.data) {
    const d = axErr.response.data;
    if (Array.isArray(d.errors) && d.errors.length) return (d.errors as string[]).join(', ');
    if (d.errors && typeof d.errors === 'object') {
      const msgs = Object.values(d.errors).flat();
      if (msgs.length) return msgs.join(', ');
    }
    if (d.detail) return d.detail;
    if (d.title) return d.title;
  }
  if (axErr.message) return axErr.message;
  return fallback;
}

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api',
  headers: { 'Content-Type': 'application/json' },
  withCredentials: true,
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

let refreshPromise: Promise<string> | null = null;

apiClient.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    // 403 PasswordChangeRequired gate
    if (error.response?.status === 403) {
      const data = error.response.data as ApiError | undefined;
      if (data?.code === 'PasswordChangeRequired') {
        window.location.href = '/auth/change-password';
        return Promise.reject(error);
      }
    }

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
