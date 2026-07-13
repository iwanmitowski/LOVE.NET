import { useInfiniteQuery, useQueryClient } from "@tanstack/react-query";
import { ChevronLeft, Clock3, Users } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Link } from "react-router";
import { getChat } from "@/api/chat";
import type { ChatMessage, RoomSummary, UserInRoom } from "@/api/types";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Bubble, BubbleContent } from "@/components/ui/bubble";
import { Marker, MarkerContent } from "@/components/ui/marker";
import {
  Message,
  MessageAvatar,
  MessageContent,
  MessageFooter,
} from "@/components/ui/message";
import {
  MessageScroller,
  MessageScrollerButton,
  MessageScrollerContent,
  MessageScrollerItem,
  MessageScrollerProvider,
  MessageScrollerViewport,
  useMessageScroller,
} from "@/components/ui/message-scroller";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useConversations } from "@/hooks/use-conversations";
import { dayLabel, messageTime } from "@/lib/dates";
import { cn } from "@/lib/utils";
import { useRealtime } from "@/realtime/connection";
import * as events from "@/realtime/events";
import { useAuthStore } from "@/stores/auth";
import Composer from "./composer";
import MembersPanel from "./members-panel";
import TypingDots from "./typing-dots";

/** Messages closer together than this collapse into one visual group. */
const GROUP_WINDOW_MS = 5 * 60_000;
const TYPING_FLASH_MS = 3_000;
const MARK_READ_DEBOUNCE_MS = 500;

interface SystemNote {
  id: string;
  text: string;
  at: string;
}

type ThreadEntry =
  | { kind: "day"; id: string; label: string }
  | { kind: "note"; id: string; text: string }
  | {
      kind: "message";
      message: ChatMessage;
      isOwn: boolean;
      groupEnd: boolean;
      showAvatar: boolean;
    };

/**
 * A single room's thread. Mounted with key={roomId} so all transient state
 * (typing, presence, live receipts) resets naturally on room change.
 */
export default function Thread({ roomId }: { roomId: string }) {
  const realtime = useRealtime();
  const queryClient = useQueryClient();
  const user = useAuthStore((s) => s.user);
  const myId = user?.id;
  const myPfp = user?.profilePicture ?? null;

  const { conversations, chatrooms, isLoading: listLoading } = useConversations();
  const conversation = conversations.find((c) => c.match.roomId === roomId);
  const chatroom = chatrooms.find((room) => room.id === roomId);
  const isPublic = !conversation && !!chatroom;

  // ---- history (pages arrive newest-first; we render oldest -> newest) ----
  const query = useInfiniteQuery({
    queryKey: ["messages", roomId],
    initialPageParam: 1,
    queryFn: ({ pageParam }) => getChat(roomId, pageParam),
    getNextPageParam: (last, pages) => {
      const loaded = pages.reduce((n, page) => n + page.messages.length, 0);
      return loaded < last.totalMessages ? pages.length + 1 : undefined;
    },
  });

  const messages = useMemo(() => {
    const flat = (query.data?.pages ?? []).flatMap((page) => page.messages);
    const seen = new Set<string>();
    const unique: ChatMessage[] = [];
    for (const message of flat) {
      if (!seen.has(message.id)) {
        seen.add(message.id);
        unique.push(message);
      }
    }
    return unique.reverse();
  }, [query.data]);

  // The summaries cache only refetches every 60s; zero the badge locally
  // whenever we tell the server this room is read.
  const clearUnread = useCallback(() => {
    queryClient.setQueryData<RoomSummary[]>(["summaries"], (summaries) =>
      summaries?.map((s) => (s.roomId === roomId ? { ...s, unreadCount: 0 } : s)),
    );
  }, [queryClient, roomId]);

  // ---- room lifecycle: active room, join, mark read, reconnect recovery ----
  useEffect(() => {
    realtime.setActiveRoom(roomId);
    realtime
      .joinRoom(roomId, myPfp)
      .then(() => realtime.markRead(roomId))
      .then(clearUnread)
      .catch((error) => console.error("Failed to join room", error));

    const offReconnected = events.on("reconnected", () => {
      realtime
        .joinRoom(roomId, myPfp)
        .then(() => realtime.markRead(roomId))
        .then(clearUnread)
        .catch((error) => console.error("Failed to re-join room", error));
      void queryClient.invalidateQueries({ queryKey: ["messages", roomId] });
    });

    return () => {
      offReconnected();
      realtime.setActiveRoom(null);
      void realtime.leaveRoom(roomId);
    };
  }, [roomId, myPfp, realtime, queryClient, clearUnread]);

  // ---- typing indicator (transient signal, ~3s flash reset per ping) ----
  const [peerTypingName, setPeerTypingName] = useState<string | null>(null);
  const typingTimer = useRef<number | null>(null);
  useEffect(() => {
    const off = events.on("peerTyping", (payload) => {
      if (payload.roomId !== roomId || payload.userId === myId) return;
      setPeerTypingName(payload.userName || "Someone");
      if (typingTimer.current) window.clearTimeout(typingTimer.current);
      typingTimer.current = window.setTimeout(() => setPeerTypingName(null), TYPING_FLASH_MS);
    });
    return () => {
      off();
      if (typingTimer.current) window.clearTimeout(typingTimer.current);
    };
  }, [roomId, myId]);

  // ---- read receipts: live event beats the (also patched) summaries cache ----
  const [liveReadAt, setLiveReadAt] = useState<string | null>(null);
  useEffect(() => {
    return events.on("readReceipt", (receipt) => {
      if (receipt.roomId === roomId) setLiveReadAt(receipt.readAt);
    });
  }, [roomId]);

  const summaryReadAt = conversation?.summary?.peerLastReadAt ?? null;
  const peerReadAt = useMemo(() => {
    const candidates = [summaryReadAt, liveReadAt].filter((v): v is string => !!v);
    if (candidates.length === 0) return null;
    return candidates.reduce((a, b) => (Date.parse(a) >= Date.parse(b) ? a : b));
  }, [summaryReadAt, liveReadAt]);

  // ---- presence + join/leave notes (public rooms) ----
  const [usersInRoom, setUsersInRoom] = useState<UserInRoom[]>([]);
  const [notes, setNotes] = useState<SystemNote[]>([]);
  const usersRef = useRef<UserInRoom[]>([]);
  useEffect(() => {
    return events.on("refreshUsersList", (payload) => {
      if (payload.roomId && payload.roomId !== roomId) return;
      const changed = findChangedUser(usersRef.current, payload.users, payload.hasLeft);
      usersRef.current = payload.users;
      setUsersInRoom(payload.users);

      if (isPublic && changed) {
        const name = changed.id === myId ? "You" : (changed.userName ?? "Someone");
        const text = payload.hasLeft
          ? `${name} left the chat.`
          : `${name} joined the chat — say hi!`;
        setNotes((prev) => [
          ...prev,
          { id: crypto.randomUUID(), text, at: new Date().toISOString() },
        ]);
      }
    });
  }, [roomId, myId, isPublic]);

  // ---- keep unread at 0 while the thread is visible (debounced re-markRead) ----
  const latestPeerMessageId = useMemo(() => {
    const last = messages[messages.length - 1];
    return last && !last.pending && last.userId !== myId ? last.id : null;
  }, [messages, myId]);

  useEffect(() => {
    if (!latestPeerMessageId) return;
    setPeerTypingName(null); // they just spoke; retire the dots early
    const timer = window.setTimeout(
      () => void realtime.markRead(roomId).then(clearUnread),
      MARK_READ_DEBOUNCE_MS,
    );
    return () => window.clearTimeout(timer);
  }, [latestPeerMessageId, roomId, realtime, clearUnread]);

  // ---- flatten into render entries: day markers, notes, grouped messages ----
  const entries = useMemo(
    () => buildEntries(messages, notes, myId),
    [messages, notes, myId],
  );

  const lastOwnId = useMemo(() => {
    for (let i = messages.length - 1; i >= 0; i--) {
      if (messages[i].userId === myId) return messages[i].id;
    }
    return null;
  }, [messages, myId]);

  const peerName = conversation?.match.userName ?? null;
  const peerMessagePicture = useMemo(() => {
    for (let i = messages.length - 1; i >= 0; i--) {
      const m = messages[i];
      if (m.userId !== myId && m.profilePicture) return m.profilePicture;
    }
    return null;
  }, [messages, myId]);
  const peerAvatarUrl =
    conversation?.match.images.find((i) => i.isProfilePicture)?.url ??
    conversation?.match.images[0]?.url ??
    peerMessagePicture;

  const title = peerName ?? chatroom?.title ?? (listLoading ? null : "Conversation");
  const headerImage = conversation ? peerAvatarUrl : chatroom?.url;

  const [membersOpen, setMembersOpen] = useState(false);
  const [sentTick, setSentTick] = useState(0);

  return (
    <div className="flex h-full min-h-0 flex-col">
      {/* Header */}
      <header className="flex h-14 shrink-0 items-center gap-2 border-b px-3">
        <Button variant="ghost" size="icon-sm" asChild className="md:hidden">
          <Link to="/messages" aria-label="Back to conversations">
            <ChevronLeft />
          </Link>
        </Button>
        {title ? (
          <>
            <Avatar className={cn(isPublic && "rounded-lg")}>
              <AvatarImage src={headerImage ?? undefined} alt="" />
              <AvatarFallback className={cn(isPublic && "rounded-lg")}>
                {isPublic ? <Users className="size-4" /> : title[0]?.toUpperCase()}
              </AvatarFallback>
            </Avatar>
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-semibold">{title}</p>
              {peerTypingName ? (
                <p className="text-primary text-xs italic">typing…</p>
              ) : isPublic && usersInRoom.length > 0 ? (
                <p className="text-muted-foreground text-xs">{usersInRoom.length} in the room</p>
              ) : null}
            </div>
            {isPublic && (
              <Button
                variant={membersOpen ? "secondary" : "ghost"}
                size="sm"
                className="gap-1.5"
                aria-label="Toggle members panel"
                onClick={() => setMembersOpen((open) => !open)}
              >
                <Users className="size-4" />
                {usersInRoom.length > 0 && <span className="text-xs">{usersInRoom.length}</span>}
              </Button>
            )}
          </>
        ) : (
          <div className="flex items-center gap-2">
            <Skeleton className="size-8 rounded-full" />
            <Skeleton className="h-4 w-28" />
          </div>
        )}
      </header>

      {/* Thread + composer share the scroller context so sends can jump to the end. */}
      <MessageScrollerProvider autoScroll defaultScrollPosition="end">
        <div className="relative flex min-h-0 flex-1">
          <div className="flex min-w-0 flex-1 flex-col">
            <div className="min-h-0 flex-1">
              {query.isLoading ? (
                <ThreadSkeleton />
              ) : (
                <MessageScroller>
                  <MessageScrollerViewport preserveScrollOnPrepend>
                    <MessageScrollerContent className="gap-0 px-4 py-4">
                      {query.hasNextPage && (
                        <LoadEarlier
                          loading={query.isFetchingNextPage}
                          onLoad={() => {
                            if (!query.isFetchingNextPage) void query.fetchNextPage();
                          }}
                        />
                      )}

                      {entries.length === 0 && (
                        <div className="text-muted-foreground m-auto py-10 text-center text-sm">
                          No messages yet — break the ice!
                        </div>
                      )}

                      {entries.map((entry) => {
                        if (entry.kind === "day") {
                          return (
                            <MessageScrollerItem key={entry.id} className="py-3">
                              <Marker variant="separator">
                                <MarkerContent className="text-xs">{entry.label}</MarkerContent>
                              </Marker>
                            </MessageScrollerItem>
                          );
                        }
                        if (entry.kind === "note") {
                          return (
                            <MessageScrollerItem key={entry.id} className="py-2">
                              <Marker variant="separator">
                                <MarkerContent className="text-xs italic">
                                  {entry.text}
                                </MarkerContent>
                              </Marker>
                            </MessageScrollerItem>
                          );
                        }

                        const m = entry.message;
                        const isLastOwn = entry.isOwn && !!conversation && m.id === lastOwnId;
                        return (
                          <MessageScrollerItem
                            key={m.id}
                            messageId={m.id}
                            className={entry.groupEnd ? "mb-3" : "mb-0.5"}
                          >
                            <Message align={entry.isOwn ? "end" : "start"}>
                              {!entry.isOwn &&
                                (entry.showAvatar ? (
                                  <MessageAvatar>
                                    <Avatar>
                                      <AvatarImage src={m.profilePicture ?? undefined} alt="" />
                                      <AvatarFallback>
                                        {(peerName?.[0] ?? "?").toUpperCase()}
                                      </AvatarFallback>
                                    </Avatar>
                                  </MessageAvatar>
                                ) : (
                                  <div aria-hidden className="w-8 shrink-0" />
                                ))}
                              <MessageContent>
                                <Bubble
                                  variant={entry.isOwn ? "default" : "muted"}
                                  align={entry.isOwn ? "end" : "start"}
                                  className={cn(m.pending && "opacity-60")}
                                >
                                  <BubbleContent
                                    className={cn(
                                      entry.isOwn && "gradient-brand border-0 text-white",
                                      m.imageUrl && "p-1",
                                    )}
                                  >
                                    {m.imageUrl ? (
                                      <img
                                        src={m.imageUrl}
                                        alt="Shared in chat"
                                        loading="lazy"
                                        className="max-h-64 max-w-full rounded-lg object-contain"
                                      />
                                    ) : (
                                      m.text
                                    )}
                                  </BubbleContent>
                                </Bubble>
                                {m.pending ? (
                                  <MessageFooter className="gap-1">
                                    <Clock3 className="size-3" />
                                    <span>Sending…</span>
                                  </MessageFooter>
                                ) : isLastOwn ? (
                                  <MessageFooter>
                                    {peerReadAt &&
                                    Date.parse(peerReadAt) >= Date.parse(m.createdOn)
                                      ? `Seen ${messageTime(peerReadAt)}`
                                      : "Delivered"}
                                  </MessageFooter>
                                ) : entry.groupEnd ? (
                                  <MessageFooter>{messageTime(m.createdOn)}</MessageFooter>
                                ) : null}
                              </MessageContent>
                            </Message>
                          </MessageScrollerItem>
                        );
                      })}

                      {peerTypingName && (
                        <div className="mt-1 flex items-end gap-2">
                          {conversation && (
                            <Avatar size="sm">
                              <AvatarImage src={peerAvatarUrl ?? undefined} alt="" />
                              <AvatarFallback>
                                {(peerName?.[0] ?? "?").toUpperCase()}
                              </AvatarFallback>
                            </Avatar>
                          )}
                          <TypingDots />
                        </div>
                      )}
                    </MessageScrollerContent>
                  </MessageScrollerViewport>
                  <MessageScrollerButton />
                </MessageScroller>
              )}
            </div>
            <ScrollToEndOnSend tick={sentTick} />
            <Composer roomId={roomId} onSent={() => setSentTick((tick) => tick + 1)} />
          </div>

          {isPublic && membersOpen && (
            <MembersPanel
              users={usersInRoom}
              className="absolute inset-y-0 right-0 z-10 shadow-lg md:static md:shadow-none"
            />
          )}
        </div>
      </MessageScrollerProvider>
    </div>
  );
}

function buildEntries(
  messages: ChatMessage[],
  notes: SystemNote[],
  myId: string | undefined,
): ThreadEntry[] {
  type Merged = { at: number } & (
    | { type: "message"; message: ChatMessage }
    | { type: "note"; note: SystemNote }
  );

  const merged: Merged[] = [
    ...messages.map((message) => ({
      type: "message" as const,
      at: Date.parse(message.createdOn),
      message,
    })),
    ...notes.map((note) => ({ type: "note" as const, at: Date.parse(note.at), note })),
  ].sort((a, b) => a.at - b.at);

  const entries: ThreadEntry[] = [];
  let prevDay = "";

  for (let i = 0; i < merged.length; i++) {
    const entry = merged[i];
    const iso = entry.type === "message" ? entry.message.createdOn : entry.note.at;
    const label = dayLabel(iso);
    if (label !== prevDay) {
      entries.push({ kind: "day", id: `day-${label}`, label });
      prevDay = label;
    }

    if (entry.type === "note") {
      entries.push({ kind: "note", id: entry.note.id, text: entry.note.text });
      continue;
    }

    const m = entry.message;
    const next = merged[i + 1];
    const groupEnd =
      !next ||
      next.type !== "message" ||
      next.message.userId !== m.userId ||
      next.at - entry.at > GROUP_WINDOW_MS ||
      dayLabel(next.message.createdOn) !== label;

    entries.push({
      kind: "message",
      message: m,
      isOwn: m.userId === myId,
      groupEnd,
      showAvatar: m.userId !== myId && groupEnd,
    });
  }

  return entries;
}

/** Old-app diffing: the changed user is the one missing from the other list. */
function findChangedUser(
  prev: UserInRoom[],
  next: UserInRoom[],
  hasLeft: boolean,
): UserInRoom | null {
  if (prev.length === 0) return null;
  const source = hasLeft ? prev : next;
  const target = new Set((hasLeft ? next : prev).map((u) => u.id));
  return source.find((u) => !target.has(u.id)) ?? null;
}

/** Top sentinel: pulls the next (older) page while it stays in view. */
function LoadEarlier({ onLoad, loading }: { onLoad: () => void; loading: boolean }) {
  const ref = useRef<HTMLDivElement>(null);
  const onLoadRef = useRef(onLoad);
  onLoadRef.current = onLoad;

  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const observer = new IntersectionObserver(
      (observed) => {
        if (observed.some((entry) => entry.isIntersecting)) onLoadRef.current();
      },
      { rootMargin: "200px 0px" },
    );
    observer.observe(el);
    return () => observer.disconnect();
  }, []);

  return (
    <div ref={ref} className="flex justify-center pb-3">
      <Button variant="ghost" size="xs" onClick={onLoad} disabled={loading}>
        {loading ? "Loading earlier messages…" : "Load earlier messages"}
      </Button>
    </div>
  );
}

/** Lives inside the scroller provider; jumps to the newest message on send. */
function ScrollToEndOnSend({ tick }: { tick: number }) {
  const { scrollToEnd } = useMessageScroller();
  const lastTick = useRef(tick);

  useEffect(() => {
    if (tick !== lastTick.current) {
      lastTick.current = tick;
      scrollToEnd({ behavior: "smooth" });
    }
  }, [tick, scrollToEnd]);

  return null;
}

function ThreadSkeleton() {
  return (
    <div className="flex h-full flex-col justify-end gap-3 p-4">
      <Skeleton className="h-10 w-48 rounded-xl" />
      <Skeleton className="ml-auto h-10 w-40 rounded-xl" />
      <Skeleton className="h-16 w-56 rounded-xl" />
      <Skeleton className="ml-auto h-10 w-52 rounded-xl" />
    </div>
  );
}
