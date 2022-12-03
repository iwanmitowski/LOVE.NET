import { globalConstants } from "../utils/constants";
import { instance } from "./api";

const baseUrl = globalConstants.API_URL + "chat";

export async function getChat(roomId, page = 1) {
  try {
    const response = await instance.post(baseUrl, {
      roomId: roomId,
      page: page,
    });
    return response.data;
  } catch (error) {
    console.log(error);
  }
}
