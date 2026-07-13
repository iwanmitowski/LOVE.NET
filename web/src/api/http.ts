import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import { API_URL } from "@/lib/constants";
import { currentToken, useAuthStore } from "@/stores/auth";

// withCredentials carries the HttpOnly refresh-token cookie set by Identity.
export const http = axios.create({
  baseURL: API_URL,
  withCredentials: true,
});

http.interceptors.request.use((config) => {
  const token = currentToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

type RetriableConfig = InternalAxiosRequestConfig & { _retry?: boolean };

// Silent refresh: on 401 (once, outside the auth endpoints themselves) rotate
// the token via the refresh cookie and replay the original request.
http.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const config = error.config as RetriableConfig | undefined;
    const url = config?.url ?? "";

    if (
      config &&
      error.response?.status === 401 &&
      !config._retry &&
      !url.includes("/identity/login") &&
      !url.includes("/identity/refreshToken")
    ) {
      config._retry = true;
      try {
        const { data } = await http.post<{ token: string }>("/identity/refreshToken");
        useAuthStore.getState().patchToken(data.token);
        config.headers.Authorization = `Bearer ${data.token}`;
        return http(config);
      } catch {
        useAuthStore.getState().logout();
      }
    }

    return Promise.reject(error);
  },
);
