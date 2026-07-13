import { MessagesSquare } from "lucide-react";
import { useState } from "react";
import { useParams } from "react-router";
import { useConversations } from "@/hooks/use-conversations";
import { cn } from "@/lib/utils";
import ConversationList from "./conversation-list";
import Thread from "./thread";

/**
 * Messenger split-pane. Desktop: list (w-80) + thread side by side.
 * Mobile: /messages shows the list, /messages/:roomId shows the thread.
 */
export default function Page() {
  const { roomId } = useParams();
  const [search, setSearch] = useState("");
  const list = useConversations(search);

  return (
    <div className="flex h-full min-h-0">
      <aside
        className={cn(
          "w-full flex-col md:flex md:w-80 md:shrink-0 md:border-r",
          roomId ? "hidden" : "flex",
        )}
      >
        <ConversationList
          conversations={list.conversations}
          chatrooms={list.chatrooms}
          search={search}
          onSearchChange={setSearch}
          isLoading={list.isLoading}
          hasNextPage={list.hasNextPage}
          onLoadMore={() => void list.fetchNextPage()}
        />
      </aside>

      <section
        className={cn("min-h-0 min-w-0 flex-1 flex-col", roomId ? "flex" : "hidden md:flex")}
      >
        {roomId ? <Thread key={roomId} roomId={roomId} /> : <EmptyThread />}
      </section>
    </div>
  );
}

function EmptyThread() {
  return (
    <div className="grid h-full place-items-center p-8 text-center">
      <div className="space-y-3">
        <span className="gradient-brand mx-auto grid size-14 place-items-center rounded-2xl">
          <MessagesSquare className="size-7 text-white" />
        </span>
        <p className="text-lg font-semibold">Pick a conversation</p>
        <p className="text-muted-foreground text-sm">
          Choose a match on the left and start something lovely.
        </p>
      </div>
    </div>
  );
}
