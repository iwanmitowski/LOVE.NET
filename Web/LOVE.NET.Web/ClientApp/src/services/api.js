import axios from "axios";
import { globalConstants } from "../utils/constants";

export const instance = axios.create({
  baseURL: globalConstants.API_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

export const axiosInternal = axios.create({
  baseURL: globalConstants.API_URL,
  headers: {
    "Content-Type": "application/json",
  },
  withCredentials: true,
});

instance.interceptors.request.use(
  (config) => {
    const auth = localStorage.getItem("auth");

    if (auth) {
      console.log(auth);
      const { token } = JSON.parse(auth);
      config.headers["Authorization"] = `Bearer ${token}`;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

axiosInternal.interceptors.request.use(
  (config) => {
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

axiosInternal.interceptors.response.use(
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
          const response = await axiosInternal.post("/identity/refreshToken");

          const { token } = response.data;
          localStorage.setItem("auth", JSON.stringify(token));

          originalConfig.headers["Authorization"] = `Bearer ${token}`;
          
          return axiosInternal(originalConfig);
        } catch (error) {
          return Promise.reject(error);
        }
      }
    }

    return Promise.reject(err);
  }
);
