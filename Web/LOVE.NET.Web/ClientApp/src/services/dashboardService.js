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

export async function moderateUser(request) {
  try {
    if (new Date(request.bannedUntil) < new Date()) {
      throw new Error("Can't ban user in the past");
    }
    await instance.post(`${baseUrl}/moderate`, request);
  } catch (error) {
    throw new Error(error?.response?.data || error?.message);
  }
}
