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

export async function getChatrooms() {
  try {
    const response = await instance.get(`${baseUrl}/chatrooms`);
    return response.data;
  } catch (error) {
    console.log(error);
  }
}

// Chat images are uploaded over REST first; the websocket message then
// carries only the returned URL (no more base64 over the socket).
export async function uploadChatImage(file) {
  const formData = new FormData();
  formData.append("file", file);

  const response = await instance.post(`${baseUrl}/images`, formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });

  return response.data.url;
}
