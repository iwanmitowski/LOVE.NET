import { globalConstants } from "../utils/constants";
import { instance } from "./api";

const baseUrl = globalConstants.API_URL + "admin/dashboard";

export async function getStatistics() {
  try {
    const response = await instance.get(baseUrl);

    return response.data;
  } catch (error) {
    console.log(error);
  }
}
