import axios from "axios";
import { globalConstants } from "../utils/constants";

import PromiseThrottle from "promise-throttle";

export const instance = axios.create({
  baseURL: globalConstants.API_URL,
  headers: {
    "Content-Type": "application/json",
  },
  withCredentials: true,
});

instance.interceptors.request.use(
  (config) => {
    const auth = localStorage.getItem("auth");

    if (!!auth) {
      const { token } = JSON.parse(auth);
      config.headers["Authorization"] = `Bearer ${token}`;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

instance.interceptors.response.use(
  (res) => {
    return res;
  },
  async (err) => {
    const originalConfig = err.config;

    if (
      !originalConfig.url.includes("/identity/login") &&
      !originalConfig.url.includes("/identity/refreshToken") &&
      err.response
    ) {
      if (err.response.status === 401 && !originalConfig._retry) {
        originalConfig._retry = true;

        try {
          const response = await instance.post("/identity/refreshToken");

          const { token } = response.data;
          localStorage.setItem("auth", JSON.stringify(token));

          originalConfig.headers["Authorization"] = `Bearer ${token}`;

          return instance(originalConfig);
        } catch (error) {
          return Promise.reject(error);
        }
      }
    }

    return Promise.reject(err);
  }
);

export const throttledAxios = new PromiseThrottle({
  requestsPerSecond: 1, // up to 1 request per second
});