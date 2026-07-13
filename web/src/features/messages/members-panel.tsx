import type { UserInRoom } from "@/api/types";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { cn } from "@/lib/utils";

/** Presence list for public chatrooms, fed by RefreshUsersList hub events. */
export default function MembersPanel({
  users,
  className,
}: {
  users: UserInRoom[];
  className?: string;
}) {
  return (
    <aside className={cn("bg-card flex w-64 shrink-0 flex-col border-l", className)}>
      <div className="border-b px-4 py-3">
        <p className="text-sm font-semibold">In the room</p>
        <p className="text-muted-foreground text-xs">
          {users.length} {users.length === 1 ? "person" : "people"} here now
        </p>
      </div>
      <ul className="min-h-0 flex-1 overflow-y-auto p-2">
        {users.length === 0 ? (
          <li className="text-muted-foreground px-3 py-4 text-xs">Nobody here yet.</li>
        ) : (
          users.map((member) => (
            <li key={member.id} className="flex items-center gap-2 rounded-lg px-2 py-1.5">
              <Avatar size="sm">
                <AvatarImage src={member.profilePictureUrl ?? undefined} alt="" />
                <AvatarFallback>{member.userName?.[0]?.toUpperCase() ?? "?"}</AvatarFallback>
              </Avatar>
              <span className="truncate text-sm">{member.userName ?? "Anonymous"}</span>
            </li>
          ))
        )}
      </ul>
    </aside>
  );
}
