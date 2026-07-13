import { Heart, MessageCircle } from "lucide-react";
import { toast } from "sonner";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import type { MatchReceived, MessageNotification } from "@/api/types";
import { router } from "@/router";

export function MatchToast({ toastId, match }: { toastId: string | number; match: MatchReceived }) {
  return (
    <div className="bg-card text-card-foreground flex w-80 items-center gap-3 rounded-xl border p-4 shadow-lg">
      <div className="relative">
        <Avatar className="size-12">
          <AvatarImage src={match.user.avatarUrl ?? undefined} />
          <AvatarFallback>{match.user.userName[0]?.toUpperCase()}</AvatarFallback>
        </Avatar>
        <span className="gradient-brand absolute -right-1 -bottom-1 grid size-5 place-items-center rounded-full">
          <Heart className="size-3 text-white" fill="currentColor" />
        </span>
      </div>
      <div className="min-w-0 flex-1">
        <p className="text-gradient-brand text-sm font-bold">It's a match!</p>
        <p className="truncate text-sm">You and {match.user.userName} liked each other.</p>
      </div>
      <Button
        size="sm"
        onClick={() => {
          toast.dismiss(toastId);
          void router.navigate(`/messages/${match.roomId}`);
        }}
      >
        Say hi
      </Button>
    </div>
  );
}

export function MessageToast({
  toastId,
  notification,
}: {
  toastId: string | number;
  notification: MessageNotification;
}) {
  return (
    <button
      className="bg-card text-card-foreground flex w-80 items-center gap-3 rounded-xl border p-4 text-left shadow-lg"
      onClick={() => {
        toast.dismiss(toastId);
        void router.navigate(`/messages/${notification.roomId}`);
      }}
    >
      <span className="bg-primary/10 text-primary grid size-10 shrink-0 place-items-center rounded-full">
        <MessageCircle className="size-5" />
      </span>
      <span className="min-w-0 flex-1">
        <span className="block text-sm font-semibold">{notification.senderUserName}</span>
        <span className="text-muted-foreground block truncate text-sm">
          {notification.text ?? "Sent a photo"}
        </span>
      </span>
    </button>
  );
}
