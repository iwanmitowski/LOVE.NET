import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
  type HubConnection,
} from "@microsoft/signalr";
import { useQueryClient, type InfiniteData } from "@tanstack/react-query";
import { createContext, useContext, useEffect, useMemo, useRef, type ReactNode } from "react";
import { toast } from "sonner";
import type {
  ChatMessage,
  ChatPage,
  MatchReceived,
  MessageNotification,
  PeerTyping,
  ReadReceipt,
  RefreshUsersList,
  RoomSummary,
} from "@/api/types";
import { BASE_URL } from "@/lib/constants";
import { MatchToast, MessageToast } from "@/realtime/toasts";
import * as events from "@/realtime/events";
import { isLogged, useAuthStore } from "@/stores/auth";

interface RealtimeApi {
  joinRoom: (roomId: string, profilePictureUrl?: string | null) => Promise<void>;
  leaveRoom: (roomId: string) => Promise<void>;
  sendMessage: (message: {
    roomId: string;
    text?: string | null;
    imageUrl?: string | null;
    profilePicture?: string | null;
  }) => Promise<void>;
  typing: (roomId: string) => void;
  markRead: (roomId: string) => Promise<void>;
  /** The room whose thread is on screen; message toasts are suppressed for it. */
  setActiveRoom: (roomId: string | null) => void;
}

const RealtimeContext = createContext<RealtimeApi | null>(null);

export function useRealtime(): RealtimeApi {
  const api = useContext(RealtimeContext);
  if (!api) throw new Error("useRealtime must be used inside <RealtimeProvider>");
  return api;
}

export function RealtimeProvider({ children }: { children: ReactNode }) {
  const logged = useAuthStore(isLogged);
  const queryClient = useQueryClient();
  const connectionRef = useRef<HubConnection | null>(null);
  const activeRoomRef = useRef<string | null>(null);

  useEffect(() => {
    if (!logged) return;

    const connection = new HubConnectionBuilder()
      .withUrl(`${BASE_URL}/chat`, {
        accessTokenFactory: () => useAuthStore.getState().user?.token ?? "",
      })
      .configureLogging(LogLevel.Warning)
      .withAutomaticReconnect()
      .build();

    const myId = useAuthStore.getState().user?.id;

    connection.on("ReceiveMessage", (message: ChatMessage) => {
      appendToThread(message);
      void queryClient.invalidateQueries({ queryKey: ["summaries"] });
    });

    connection.on("MessageNotification", (notification: MessageNotification) => {
      void queryClient.invalidateQueries({ queryKey: ["summaries"] });

      if (notification.userId !== myId && activeRoomRef.current !== notification.roomId) {
        toast.custom((id) => <MessageToast toastId={id} notification={notification} />, {
          id: `msg-${notification.roomId}`,
        });
      }
    });

    connection.on("MatchReceived", (match: MatchReceived) => {
      void queryClient.invalidateQueries({ queryKey: ["matches"] });
      toast.custom((id) => <MatchToast toastId={id} match={match} />, {
        id: `match-${match.matchId}`,
        duration: 10_000,
      });
    });

    connection.on("PeerTyping", (payload: PeerTyping) => events.emit("peerTyping", payload));

    connection.on("ReadReceipt", (receipt: ReadReceipt) => {
      events.emit("readReceipt", receipt);
      queryClient.setQueryData<RoomSummary[]>(["summaries"], (summaries) =>
        summaries?.map((s) =>
          s.roomId === receipt.roomId ? { ...s, peerLastReadAt: receipt.readAt } : s,
        ),
      );
    });

    connection.on("RefreshUsersList", (payload: RefreshUsersList) =>
      events.emit("refreshUsersList", payload),
    );

    connection.onreconnecting(() => {
      toast.loading("Reconnecting…", { id: "hub-reconnect" });
    });
    connection.onreconnected(() => {
      toast.dismiss("hub-reconnect");
      events.emit("reconnected", undefined);
    });

    connection.start().catch((error) => {
      console.error("Realtime connection failed", error);
    });
    connectionRef.current = connection;

    return () => {
      connectionRef.current = null;
      void connection.stop();
    };
  }, [logged, queryClient]);

  function appendToThread(message: ChatMessage) {
    queryClient.setQueryData<InfiniteData<ChatPage>>(["messages", message.roomId], (data) => {
      if (!data || data.pages.length === 0) return data;

      const already = data.pages.some((p) => p.messages.some((m) => m.id === message.id));
      if (already) return data;

      const [first, ...rest] = data.pages;

      // Replace the matching optimistic bubble (same author + body) if present.
      const pendingIndex = first.messages.findIndex(
        (m) => m.pending && m.userId === message.userId &&
          m.text === message.text && m.imageUrl === message.imageUrl,
      );

      const messages =
        pendingIndex >= 0
          ? first.messages.map((m, i) => (i === pendingIndex ? { ...message } : m))
          : [message, ...first.messages];

      return {
        ...data,
        pages: [
          { messages, totalMessages: first.totalMessages + (pendingIndex >= 0 ? 0 : 1) },
          ...rest,
        ],
      };
    });
  }

  const api = useMemo<RealtimeApi>(
    () => ({
      joinRoom: async (roomId, profilePictureUrl = null) => {
        await ready();
        await connectionRef.current!.invoke("JoinRoom", { roomId, profilePictureUrl });
      },
      leaveRoom: async (roomId) => {
        if (connectionRef.current?.state === HubConnectionState.Connected) {
          await connectionRef.current.invoke("LeaveRoom", { roomId, profilePictureUrl: null });
        }
      },
      sendMessage: async (message) => {
        await ready();
        await connectionRef.current!.invoke("SendMessage", {
          roomId: message.roomId,
          text: message.text ?? null,
          imageUrl: message.imageUrl ?? null,
          profilePicture: message.profilePicture ?? null,
        });
      },
      typing: (roomId) => {
        if (connectionRef.current?.state === HubConnectionState.Connected) {
          void connectionRef.current.send("Typing", roomId);
        }
      },
      markRead: async (roomId) => {
        if (connectionRef.current?.state === HubConnectionState.Connected) {
          await connectionRef.current.invoke("MarkRead", roomId);
        }
      },
      setActiveRoom: (roomId) => {
        activeRoomRef.current = roomId;
      },
    }),
    [],
  );

  async function ready(): Promise<void> {
    const connection = connectionRef.current;
    if (!connection) throw new Error("Realtime connection is not available");

    if (connection.state === HubConnectionState.Disconnected) {
      await connection.start();
    }

    // Wait out an in-flight (re)connect instead of failing the invoke.
    const deadline = Date.now() + 10_000;
    while (connection.state !== HubConnectionState.Connected && Date.now() < deadline) {
      await new Promise((resolve) => setTimeout(resolve, 100));
    }
  }

  return <RealtimeContext.Provider value={api}>{children}</RealtimeContext.Provider>;
}
