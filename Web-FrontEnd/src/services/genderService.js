import { instance } from "../services/api";
import { globalConstants } from "../utils/constants";

const baseUrl = globalConstants.API_URL + "genders";

export async function getAll() {
  const response = await instance.get(`${baseUrl}`);

  return response.data;
}
