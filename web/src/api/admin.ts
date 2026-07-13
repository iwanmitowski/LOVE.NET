import { http } from "@/api/http";
import type { AdminUsersResponse, Statistics } from "@/api/types";

export async function getStatistics(): Promise<Statistics> {
  const { data } = await http.get<Statistics>("/admin/dashboard");
  return data;
}

export async function getUsers(input: {
  page: number;
  search?: string;
  showBanned?: boolean;
}): Promise<AdminUsersResponse> {
  const { data } = await http.post<AdminUsersResponse>("/admin/users", input);
  return data;
}

export async function moderateUser(input: {
  userId: string;
  bannedUntil: string | null;
}): Promise<void> {
  await http.post("/admin/moderate", input);
}
