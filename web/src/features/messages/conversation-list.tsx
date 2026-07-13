import { Heart, Search, Users } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { NavLink } from "react-router";
import type { Chatroom } from "@/api/types";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import type { Conversation } from "@/hooks/use-conversations";
import { conversationTime } from "@/lib/dates";
import { cn } from "@/lib/utils";
import * as events from "@/realtime/events";
import { useAuthStore } from "@/stores/auth";

const TYPING_FLASH_MS = 3_000;

interface ConversationListProps {
  conversations: Conversation[];
  chatrooms: Chatroom[];
  search: string;
  onSearchChange: (value: string) => void;
  isLoading: boolean;
  hasNextPage: boolean;
  onLoadMore: () => void;
}

/** Left column: search, match conversations, and public "Communities" rooms. */
export default function ConversationList({
  conversations,
  chatrooms,
  search,
  onSearchChange,
  isLoading,
  hasNextPage,
  onLoadMore,
}: ConversationListProps) {
  const myId = useAuthStore((s) => s.user?.id);
  const typingRooms = useTypingRooms(myId);
  const rows = conversations.filter((c) => !!c.match.roomId);

  return (
    <div className="flex h-full min-h-0 flex-col">
      <div className="shrink-0 p-3 pb-2">
        <h1 className="px-1 pb-2 text-lg font-bold md:hidden">Messages</h1>
        <div className="relative">
          <Search className="text-muted-foreground pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2" />
          <Input
            value={search}
            onChange={(e) => onSearchChange(e.target.value)}
            placeholder="Search matches"
            aria-label="Search matches"
            className="rounded-full pl-9"
          />
        </div>
      </div>

      <div className="min-h-0 flex-1 overflow-y-auto px-2 pb-4">
        {isLoading ? (
          <RowSkeletons />
        ) : rows.length === 0 ? (
          <div className="text-muted-foreground flex flex-col items-center gap-2 px-4 py-10 text-center text-sm">
            <Heart className="size-6" />
            {search ? "No matches for that search." : "No conversations yet — go match with someone!"}
          </div>
        ) : (
          <ul className="flex flex-col gap-0.5">
            {rows.map((conversation) => (
              <li key={conversation.match.id}>
                <MatchRow
                  conversation={conversation}
                  typing={!!typingRooms[conversation.match.roomId!]}
                  myId={myId}
                />
              </li>
            ))}
          </ul>
        )}
        {hasNextPage && !isLoading && <LoadMoreSentinel onLoadMore={onLoadMore} />}

        {chatrooms.length > 0 && (
          <>
            <p className="text-muted-foreground px-3 pt-5 pb-1 text-xs font-semibold tracking-wide uppercase">
              Communities
            </p>
            <ul className="flex flex-col gap-0.5">
              {chatrooms.map((room) => (
                <li key={room.id}>
                  <ChatroomRow room={room} typing={!!typingRooms[room.id]} />
                </li>
              ))}
            </ul>
          </>
        )}
      </div>
    </div>
  );
}

/** Rooms with a live peerTyping signal in the last ~3s. */
function useTypingRooms(myId: string | undefined): Record<string, boolean> {
  const [typingRooms, setTypingRooms] = useState<Record<string, boolean>>({});
  const timers = useRef(new Map<string, number>());

  useEffect(() => {
    const off = events.on("peerTyping", (payload) => {
      if (payload.userId === myId) return;
      setTypingRooms((prev) => (prev[payload.roomId] ? prev : { ...prev, [payload.roomId]: true }));

      const existing = timers.current.get(payload.roomId);
      if (existing) window.clearTimeout(existing);
      timers.current.set(
        payload.roomId,
        window.setTimeout(() => {
          timers.current.delete(payload.roomId);
          setTypingRooms((prev) => {
            const next = { ...prev };
            delete next[payload.roomId];
            return next;
          });
        }, TYPING_FLASH_MS),
      );
    });

    const currentTimers = timers.current;
    return () => {
      off();
      for (const timer of currentTimers.values()) window.clearTimeout(timer);
      currentTimers.clear();
    };
  }, [myId]);

  return typingRooms;
}

function MatchRow({
  conversation,
  typing,
  myId,
}: {
  conversation: Conversation;
  typing: boolean;
  myId: string | undefined;
}) {
  const { match, summary } = conversation;
  const picture = match.images.find((i) => i.isProfilePicture)?.url ?? match.images[0]?.url;
  const unread = summary?.unreadCount ?? 0;

  const mine = !!summary?.lastMessageSenderId && summary.lastMessageSenderId === myId;
  const body = summary?.lastMessageText ?? (summary?.lastMessageImageUrl ? "📷 Photo" : null);
  const preview = typing ? "typing…" : body ? (mine ? `You: ${body}` : body) : "You matched — say hi!";

  return (
    <NavLink to={`/messages/${match.roomId}`} className={rowClass}>
      <Avatar size="lg">
        <AvatarImage src={picture ?? undefined} alt="" />
        <AvatarFallback>{match.userName[0]?.toUpperCase()}</AvatarFallback>
      </Avatar>
      <div className="min-w-0 flex-1">
        <div className="flex items-baseline justify-between gap-2">
          <span className="truncate text-sm font-semibold">{match.userName}</span>
          {summary?.lastMessageAt && (
            <span className="text-muted-foreground shrink-0 text-[11px]">
              {conversationTime(summary.lastMessageAt)}
            </span>
          )}
        </div>
        <div className="mt-0.5 flex items-center justify-between gap-2">
          <span
            className={cn(
              "truncate text-xs",
              typing
                ? "text-primary italic"
                : unread > 0
                  ? "text-foreground font-medium"
                  : "text-muted-foreground",
            )}
          >
            {preview}
          </span>
          {unread > 0 && (
            <Badge className="gradient-brand h-5 min-w-5 shrink-0 border-0 px-1.5 text-white">
              {unread > 9 ? "9+" : unread}
            </Badge>
          )}
        </div>
      </div>
    </NavLink>
  );
}

function ChatroomRow({ room, typing }: { room: Chatroom; typing: boolean }) {
  return (
    <NavLink to={`/messages/${room.id}`} className={rowClass}>
      <Avatar size="lg" className="rounded-lg">
        <AvatarImage src={room.url} alt="" />
        <AvatarFallback className="rounded-lg">
          <Users className="size-4" />
        </AvatarFallback>
      </Avatar>
      <div className="min-w-0 flex-1">
        <span className="block truncate text-sm font-semibold">{room.title}</span>
        <span
          className={cn(
            "mt-0.5 block truncate text-xs",
            typing ? "text-primary italic" : "text-muted-foreground",
          )}
        >
          {typing ? "typing…" : "Public room"}
        </span>
      </div>
    </NavLink>
  );
}

function rowClass({ isActive }: { isActive: boolean }) {
  return cn(
    "flex items-center gap-3 rounded-xl px-3 py-2.5 transition-colors",
    isActive ? "bg-primary/10" : "hover:bg-accent",
  );
}

function LoadMoreSentinel({ onLoadMore }: { onLoadMore: () => void }) {
  const ref = useRef<HTMLDivElement>(null);
  const onLoadMoreRef = useRef(onLoadMore);
  onLoadMoreRef.current = onLoadMore;

  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) onLoadMoreRef.current();
      },
      { rootMargin: "120px 0px" },
    );
    observer.observe(el);
    return () => observer.disconnect();
  }, []);

  return (
    <div ref={ref} className="text-muted-foreground py-3 text-center text-xs">
      Loading more…
    </div>
  );
}

function RowSkeletons() {
  return (
    <div className="flex flex-col gap-1 px-1 py-1">
      {Array.from({ length: 6 }, (_, i) => (
        <div key={i} className="flex items-center gap-3 px-2 py-2.5">
          <Skeleton className="size-10 rounded-full" />
          <div className="flex-1 space-y-2">
            <Skeleton className="h-3.5 w-2/5" />
            <Skeleton className="h-3 w-4/5" />
          </div>
        </div>
      ))}
    </div>
  );
}
