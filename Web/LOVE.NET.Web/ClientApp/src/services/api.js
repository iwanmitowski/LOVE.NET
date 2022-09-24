import axios from "axios";
import { globalConstants } from "../utils/constants";

export default axios.create({
  baseURL: globalConstants.API_URL
});

export const axiosInternal = axios.create({
    baseURL: globalConstants.API_URL,
    headers: {
      "Content-Type": "application/json",
    },
    withCredentials: true,
});