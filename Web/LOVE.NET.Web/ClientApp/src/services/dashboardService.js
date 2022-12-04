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

export async function getUsers(request) {
  try {
    const response = await instance.post(`${baseUrl}/users`, request);

    return response.data;
  } catch (error) {
    console.log(error);
  }
}
