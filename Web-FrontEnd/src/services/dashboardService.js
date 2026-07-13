import { globalConstants } from "../utils/constants";
import { instance } from "./api";

// Stats are aggregated by the gateway; users live in Profile, moderation in Identity.
const adminBaseUrl = globalConstants.API_URL + "admin";

export async function getStatistics() {
  try {
    const response = await instance.get(`${adminBaseUrl}/dashboard`);

    return response.data;
  } catch (error) {
    console.log(error);
  }
}

export async function getUsers(request) {
  try {
    const response = await instance.post(`${adminBaseUrl}/users`, request);

    return response.data;
  } catch (error) {
    console.log(error);
  }
}

export async function moderateUser(request) {
  try {
    if (request.bannedUntil && new Date(request.bannedUntil) < new Date()) {
      throw new Error("Can't ban user in the past");
    }
    await instance.post(`${adminBaseUrl}/moderate`, request);
  } catch (error) {
    throw new Error(error?.response?.data || error?.message);
  }
}
