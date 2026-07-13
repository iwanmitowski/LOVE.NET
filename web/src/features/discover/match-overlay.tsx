import { Heart } from "lucide-react";
import { motion } from "motion/react";
import { useNavigate } from "react-router";
import type { UserCard } from "@/api/types";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { useAuthStore } from "@/stores/auth";

interface MatchOverlayProps {
  user: UserCard;
  onDismiss: () => void;
}

/** Full-screen celebration shown when a right-swipe comes back as a mutual match. */
export default function MatchOverlay({ user, onDismiss }: MatchOverlayProps) {
  const me = useAuthStore((s) => s.user);
  const navigate = useNavigate();
  const theirPhoto = user.images.find((image) => image.isProfilePicture) ?? user.images[0];

  return (
    <motion.div
      className="fixed inset-0 z-50 grid place-items-center bg-black/70 p-6 backdrop-blur-sm"
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
    >
      <motion.div
        className="flex w-full max-w-sm flex-col items-center gap-8 text-center"
        initial={{ scale: 0.8, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        exit={{ scale: 0.9, opacity: 0 }}
        transition={{ type: "spring", stiffness: 260, damping: 22 }}
      >
        <div className="space-y-2">
          <h2 className="text-gradient-brand text-5xl font-extrabold tracking-tight">
            It's a match!
          </h2>
          <p className="text-white/80">You and {user.userName} liked each other.</p>
        </div>

        <div className="relative flex items-center">
          <Avatar className="size-24 border-4 border-white shadow-xl">
            <AvatarImage src={me?.profilePicture ?? undefined} />
            <AvatarFallback className="text-2xl">
              {me?.userName?.[0]?.toUpperCase()}
            </AvatarFallback>
          </Avatar>
          <Avatar className="-ml-5 size-24 border-4 border-white shadow-xl">
            <AvatarImage src={theirPhoto?.url} />
            <AvatarFallback className="text-2xl">{user.userName[0]?.toUpperCase()}</AvatarFallback>
          </Avatar>
          <span className="gradient-brand absolute top-full left-1/2 grid size-10 -translate-x-1/2 -translate-y-1/2 place-items-center rounded-full border-2 border-white shadow-lg">
            <Heart className="size-5 text-white" fill="currentColor" />
          </span>
        </div>

        <div className="flex w-full flex-col gap-2">
          <Button
            size="lg"
            className="gradient-brand w-full border-0 text-white hover:opacity-90"
            onClick={() => void navigate(user.roomId ? `/messages/${user.roomId}` : "/messages")}
          >
            Send a message
          </Button>
          <Button
            size="lg"
            variant="outline"
            className="w-full border-white/40 bg-transparent text-white hover:bg-white/10 hover:text-white"
            onClick={onDismiss}
          >
            Keep swiping
          </Button>
        </div>
      </motion.div>
    </motion.div>
  );
}
