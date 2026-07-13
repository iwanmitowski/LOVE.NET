import { http } from "@/api/http";
import type { ChatPage, Chatroom, RoomSummary } from "@/api/types";

export async function getChat(roomId: string, page = 1): Promise<ChatPage> {
  const { data } = await http.post<ChatPage>("/chat", { roomId, page });
  return data;
}

export async function getChatrooms(): Promise<Chatroom[]> {
  const { data } = await http.get<Chatroom[]>("/chat/chatrooms");
  return data;
}

export async function getSummaries(roomIds: string[]): Promise<RoomSummary[]> {
  if (roomIds.length === 0) return [];
  const { data } = await http.post<RoomSummary[]>("/chat/summaries", { roomIds });
  return data;
}

/** Upload first, then send the returned URL over the hub (field name "file" is the contract). */
export async function uploadChatImage(file: File): Promise<string> {
  const form = new FormData();
  form.append("file", file);
  const { data } = await http.post<{ url: string }>("/chat/images", form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data.url;
}
