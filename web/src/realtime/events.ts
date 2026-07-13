import type { PeerTyping, ReadReceipt, RefreshUsersList } from "@/api/types";

// Transient hub signals that shouldn't live in the query cache: thread
// components subscribe directly (typing dots, presence, seen ticks).
type EventMap = {
  peerTyping: PeerTyping;
  readReceipt: ReadReceipt;
  refreshUsersList: RefreshUsersList & { roomId?: string };
  reconnected: void;
};

type Handler<T> = (payload: T) => void;

const handlers: { [K in keyof EventMap]: Set<Handler<EventMap[K]>> } = {
  peerTyping: new Set(),
  readReceipt: new Set(),
  refreshUsersList: new Set(),
  reconnected: new Set(),
};

export function on<K extends keyof EventMap>(event: K, handler: Handler<EventMap[K]>): () => void {
  handlers[event].add(handler);
  return () => handlers[event].delete(handler);
}

export function emit<K extends keyof EventMap>(event: K, payload: EventMap[K]): void {
  for (const handler of handlers[event]) {
    handler(payload);
  }
}
