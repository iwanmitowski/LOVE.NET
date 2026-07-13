import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { getMatches } from "@/api/dating";
import { getChatrooms, getSummaries } from "@/api/chat";
import type { RoomSummary, UserCard } from "@/api/types";
import { PAGE_SIZE } from "@/lib/constants";
import { useAuthStore } from "@/stores/auth";

export interface Conversation {
  match: UserCard;
  summary: RoomSummary | null;
}

/**
 * Conversation-list data shared by the Messages page and the nav unread badge:
 * matches (paged) joined with the Chat service's room summaries.
 */
export function useConversations(search = "") {
  const userId = useAuthStore((s) => s.user?.id);

  const matchesQuery = useInfiniteQuery({
    queryKey: ["matches", search],
    enabled: !!userId,
    initialPageParam: 1,
    queryFn: ({ pageParam }) =>
      getMatches({ userId: userId!, page: pageParam, search: search || undefined }),
    getNextPageParam: (last, pages) => {
      const loaded = pages.reduce((n, p) => n + p.matches.length, 0);
      return loaded < last.totalMatches ? pages.length + 1 : undefined;
    },
  });

  const matches = (matchesQuery.data?.pages ?? [])
    .flatMap((p) => p.matches)
    .filter((m, i, all) => all.findIndex((x) => x.id === m.id) === i);

  const roomIds = matches.map((m) => m.roomId).filter((id): id is string => !!id);

  const summariesQuery = useQuery({
    queryKey: ["summaries"],
    enabled: roomIds.length > 0,
    queryFn: () => getSummaries(roomIds),
    refetchInterval: 60_000,
  });

  const chatroomsQuery = useQuery({
    queryKey: ["chatrooms"],
    enabled: !!userId,
    queryFn: getChatrooms,
    staleTime: 5 * 60_000,
  });

  const summariesByRoom = new Map((summariesQuery.data ?? []).map((s) => [s.roomId, s]));

  const conversations: Conversation[] = matches.map((match) => ({
    match,
    summary: match.roomId ? (summariesByRoom.get(match.roomId) ?? null) : null,
  }));

  const totalUnread = (summariesQuery.data ?? []).reduce((n, s) => n + s.unreadCount, 0);

  return {
    conversations,
    chatrooms: chatroomsQuery.data ?? [],
    totalUnread,
    isLoading: matchesQuery.isLoading,
    fetchNextPage: matchesQuery.fetchNextPage,
    hasNextPage: matchesQuery.hasNextPage ?? false,
    pageSize: PAGE_SIZE,
  };
}
