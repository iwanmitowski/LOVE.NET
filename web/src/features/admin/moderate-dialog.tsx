import { useMutation } from "@tanstack/react-query";
import dayjs from "dayjs";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { moderateUser } from "@/api/admin";
import type { AccountDetails } from "@/api/types";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { errorMessages } from "@/lib/errors";

interface ModerateDialogProps {
  /** The user being moderated; null keeps the dialog closed. */
  user: AccountDetails | null;
  onClose: () => void;
  /** Called after the API confirms, so the caller can patch its cache. */
  onModerated: (userId: string, isBanned: boolean) => void;
}

export default function ModerateDialog({ user, onClose, onModerated }: ModerateDialogProps) {
  const today = dayjs().format("YYYY-MM-DD");
  const [bannedUntil, setBannedUntil] = useState(today);
  const isBan = !!user && !user.isBanned;

  // Reset per-target state whenever a new user is picked.
  const userId = user?.id;
  useEffect(() => {
    if (userId) setBannedUntil(dayjs().format("YYYY-MM-DD"));
  }, [userId]);

  const mutation = useMutation({
    mutationFn: (input: { userId: string; bannedUntil: string | null }) => moderateUser(input),
    onSuccess: (_, input) => {
      const banned = input.bannedUntil !== null;
      toast.success(banned ? "User banned" : "User unbanned");
      onModerated(input.userId, banned);
      onClose();
    },
  });

  function onConfirm() {
    if (!user) return;
    mutation.mutate({
      userId: user.id,
      bannedUntil: isBan ? dayjs(bannedUntil).toISOString() : null,
    });
  }

  return (
    <Dialog open={!!user} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>{isBan ? "Ban user" : "Unban user"}</DialogTitle>
          <DialogDescription>
            {isBan
              ? `${user?.userName ?? "This user"} will be banned until the selected date.`
              : `${user?.userName ?? "This user"} will regain access immediately.`}
          </DialogDescription>
        </DialogHeader>

        {isBan && (
          <div className="space-y-2">
            <Label htmlFor="banned-until">Banned until</Label>
            <Input
              id="banned-until"
              type="date"
              min={today}
              value={bannedUntil}
              onChange={(e) => setBannedUntil(e.target.value)}
            />
          </div>
        )}

        {mutation.isError && (
          <div className="text-destructive space-y-0.5 text-sm">
            {errorMessages(mutation.error).map((line) => (
              <p key={line}>{line}</p>
            ))}
          </div>
        )}

        <DialogFooter>
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button
            variant={isBan ? "destructive" : "default"}
            onClick={onConfirm}
            disabled={mutation.isPending || (isBan && !bannedUntil)}
          >
            {mutation.isPending ? "Saving…" : isBan ? "Ban" : "Unban"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
