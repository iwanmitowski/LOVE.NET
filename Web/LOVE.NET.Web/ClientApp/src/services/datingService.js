import { globalConstants } from "../utils/constants";
import { instance } from "./api";
import axiosThrottle from 'axios-request-throttle';

const baseUrl = globalConstants.API_URL + "dating";

export async function getUsersToSwipe() {
    try {
        const response = await instance.get(`${baseUrl}`);

        return response.data;
      } catch (error) {
        throw new Error(error);
      }
}

export async function likeUser(userId) {
  try {
      const response = await axiosThrottle.use(instance.post(`${baseUrl}/like/${userId}`), { requestsPerSecond: 5 });
  
      return response.data;
    } catch (error) {
      throw new Error(error);
    }
}