import { useQuery } from "@tanstack/react-query";
import {
  Ban,
  Heart,
  Image as ImageIcon,
  Layers,
  MessageSquare,
  ThumbsUp,
  Users,
} from "lucide-react";
import { Link } from "react-router";
import { getStatistics } from "@/api/admin";
import type { Statistics } from "@/api/types";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { errorMessages } from "@/lib/errors";

interface StatDef {
  key: keyof Statistics;
  label: string;
  icon: typeof Users;
  to?: string;
}

const stats: StatDef[] = [
  { key: "usersCount", label: "Users", icon: Users, to: "/admin/users" },
  { key: "bannedUsersCount", label: "Banned users", icon: Ban, to: "/admin/users?banned=1" },
  { key: "matchesCount", label: "Matches", icon: Heart },
  { key: "likedUsersCount", label: "Likes", icon: ThumbsUp },
  { key: "notSwipedUsersCount", label: "Not swiped users", icon: Layers },
  { key: "imagesCount", label: "Images", icon: ImageIcon },
  { key: "messagesCount", label: "Messages", icon: MessageSquare },
];

export default function DashboardPage() {
  const query = useQuery({ queryKey: ["admin", "stats"], queryFn: getStatistics });

  return (
    <div className="mx-auto w-full max-w-5xl p-4 md:p-8">
      <h1 className="text-2xl font-bold tracking-tight">Dashboard</h1>
      <p className="text-muted-foreground mt-1 text-sm">
        A live snapshot of the LOVE.NET community.
      </p>

      {query.isError && (
        <div className="text-destructive mt-6 space-y-0.5 text-sm">
          {errorMessages(query.error).map((line) => (
            <p key={line}>{line}</p>
          ))}
        </div>
      )}

      <div className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {query.isPending &&
          stats.map((stat) => (
            <Card key={stat.key}>
              <CardContent className="flex items-center gap-4 pt-6">
                <Skeleton className="size-11 rounded-xl" />
                <div className="space-y-2">
                  <Skeleton className="h-4 w-24" />
                  <Skeleton className="h-7 w-16" />
                </div>
              </CardContent>
            </Card>
          ))}

        {query.data &&
          stats.map((stat) => <StatCard key={stat.key} stat={stat} value={query.data[stat.key]} />)}
      </div>
    </div>
  );
}

function StatCard({ stat, value }: { stat: StatDef; value: number }) {
  const card = (
    <Card
      className={
        stat.to
          ? "hover:border-primary/50 h-full transition-colors hover:shadow-md"
          : "h-full"
      }
    >
      <CardContent className="flex items-center gap-4 pt-6">
        <span className="bg-primary/10 text-primary grid size-11 shrink-0 place-items-center rounded-xl">
          <stat.icon className="size-5" />
        </span>
        <div className="min-w-0">
          <p className="text-muted-foreground truncate text-sm">{stat.label}</p>
          <p className="text-3xl font-semibold">{value.toLocaleString()}</p>
        </div>
      </CardContent>
    </Card>
  );

  if (!stat.to) return card;

  return (
    <Link to={stat.to} className="focus-visible:ring-ring rounded-xl outline-none focus-visible:ring-2">
      {card}
    </Link>
  );
}
