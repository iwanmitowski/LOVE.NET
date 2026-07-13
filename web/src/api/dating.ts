import { http } from "@/api/http";
import type { LikeResponse, MatchesResponse, UserCard } from "@/api/types";

export async function getUsersToSwipe(): Promise<UserCard[]> {
  const { data } = await http.get<UserCard[]>("/dating");
  return data;
}

export async function getMatches(input: {
  userId: string;
  page: number;
  search?: string;
}): Promise<MatchesResponse> {
  const { data } = await http.post<MatchesResponse>("/dating/matches", input);
  return data;
}

let lastLikeAt = 0;

/** Client-side spacing so bursts of swipes respect the gateway's like rate limit. */
export async function likeUser(userId: string): Promise<LikeResponse> {
  const now = Date.now();
  const wait = Math.max(0, lastLikeAt + 250 - now);
  lastLikeAt = now + wait;
  if (wait > 0) {
    await new Promise((resolve) => setTimeout(resolve, wait));
  }

  const { data } = await http.post<LikeResponse>(`/dating/like/${userId}`);
  return data;
}
