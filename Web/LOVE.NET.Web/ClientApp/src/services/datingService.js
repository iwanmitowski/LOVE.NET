import { globalConstants } from "../utils/constants";
import { instance, throttledAxios } from "./api";

const baseUrl = globalConstants.API_URL + "dating";

export async function getUsersToSwipe() {
    try {
        const response = await instance.get(`${baseUrl}`);

        return response.data;
      } catch (error) {
        throw new Error(error);
      }
}

export async function getMatches(userId) {
  try {
    const response = await instance.get(`${baseUrl}/matches/?id=${userId}`);

    return response.data;
  } catch (error) {
    throw new Error(error);
  }
}

export async function likeUser(userId) {
  try {
      // Throttle reduces the backend calls due to bug in react-tinder-card
      const response = await throttledAxios.add(() => instance.post(`${baseUrl}/like/${userId}`));
  
      return response.data;
    } catch (error) {
      throw new Error(error.message);
    }
}