import { globalConstants } from "../utils/constants";
import { instance } from "./api";

const baseUrl = globalConstants.API_URL + "dating";

export async function getUsersToSwipe() {
    try {
        const response = await instance.get(`${baseUrl}`);
    
        return response.data.result;
      } catch (error) {
        throw new Error(error);
      }
}