import {
  useInfiniteQuery,
  useQueryClient,
  type InfiniteData,
} from "@tanstack/react-query";
import { Search } from "lucide-react";
import { useEffect, useState } from "react";
import { useSearchParams } from "react-router";
import { getUsers } from "@/api/admin";
import type { AccountDetails, AdminUsersResponse } from "@/api/types";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ageFrom } from "@/lib/dates";
import { errorMessages } from "@/lib/errors";
import ModerateDialog from "./moderate-dialog";

export default function UsersPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [showBanned, setShowBanned] = useState(searchParams.get("banned") === "1");
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [moderateTarget, setModerateTarget] = useState<AccountDetails | null>(null);
  const queryClient = useQueryClient();

  // Debounce the free-text search into the query key.
  useEffect(() => {
    const handle = setTimeout(() => setSearch(searchInput.trim()), 300);
    return () => clearTimeout(handle);
  }, [searchInput]);

  const query = useInfiniteQuery({
    queryKey: ["admin", "users", search, showBanned],
    queryFn: ({ pageParam }) =>
      getUsers({ page: pageParam, search: search || undefined, showBanned }),
    initialPageParam: 1,
    getNextPageParam: (lastPage, allPages) => {
      const loaded = allPages.reduce((count, page) => count + page.users.length, 0);
      return loaded < lastPage.totalUsers ? allPages.length + 1 : undefined;
    },
  });

  const users = query.data?.pages.flatMap((page) => page.users) ?? [];

  function onTabChange(value: string) {
    const banned = value === "banned";
    setShowBanned(banned);
    setSearchParams(banned ? { banned: "1" } : {}, { replace: true });
  }

  function onModerated(userId: string, isBanned: boolean) {
    // Patch the row in every cached filter combination.
    queryClient.setQueriesData<InfiniteData<AdminUsersResponse>>(
      { queryKey: ["admin", "users"] },
      (old) =>
        old && {
          ...old,
          pages: old.pages.map((page) => ({
            ...page,
            users: page.users.map((u) => (u.id === userId ? { ...u, isBanned } : u)),
          })),
        },
    );
  }

  return (
    <div className="mx-auto w-full max-w-5xl p-4 md:p-8">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Users</h1>
          <p className="text-muted-foreground mt-1 text-sm">
            Search, review, and moderate the community.
          </p>
        </div>
        <div className="flex items-center gap-3">
          <div className="relative">
            <Search className="text-muted-foreground absolute top-1/2 left-2.5 size-4 -translate-y-1/2" />
            <Input
              className="w-full pl-8 sm:w-56"
              placeholder="Search users…"
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
            />
          </div>
          <Tabs value={showBanned ? "banned" : "all"} onValueChange={onTabChange}>
            <TabsList>
              <TabsTrigger value="all">All</TabsTrigger>
              <TabsTrigger value="banned">Banned</TabsTrigger>
            </TabsList>
          </Tabs>
        </div>
      </div>

      {query.isError && (
        <div className="text-destructive mt-6 space-y-0.5 text-sm">
          {errorMessages(query.error).map((line) => (
            <p key={line}>{line}</p>
          ))}
        </div>
      )}

      <div className="mt-6 rounded-xl border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>User</TableHead>
              <TableHead>Email</TableHead>
              <TableHead>Location</TableHead>
              <TableHead className="text-right">Age</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="w-24" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {query.isPending && <SkeletonRows />}

            {!query.isPending && users.length === 0 && !query.isError && (
              <TableRow>
                <TableCell colSpan={6} className="text-muted-foreground h-24 text-center">
                  No users found.
                </TableCell>
              </TableRow>
            )}

            {users.map((user) => (
              <UserRow key={user.id} user={user} onModerate={() => setModerateTarget(user)} />
            ))}
          </TableBody>
        </Table>
      </div>

      {query.hasNextPage && (
        <div className="mt-4 flex justify-center">
          <Button
            variant="outline"
            onClick={() => void query.fetchNextPage()}
            disabled={query.isFetchingNextPage}
          >
            {query.isFetchingNextPage ? "Loading…" : "Load more"}
          </Button>
        </div>
      )}

      <ModerateDialog
        user={moderateTarget}
        onClose={() => setModerateTarget(null)}
        onModerated={onModerated}
      />
    </div>
  );
}

function UserRow({ user, onModerate }: { user: AccountDetails; onModerate: () => void }) {
  const profilePicture =
    user.images.find((image) => image.isProfilePicture) ?? user.images[0];
  const location =
    [user.cityName, user.countryName].filter(Boolean).join(", ") || "—";

  return (
    <TableRow>
      <TableCell>
        <div className="flex items-center gap-2.5">
          <Avatar className="size-8">
            <AvatarImage src={profilePicture?.url} alt={user.userName} />
            <AvatarFallback>{user.userName[0]?.toUpperCase()}</AvatarFallback>
          </Avatar>
          <span className="max-w-40 truncate font-medium">{user.userName}</span>
        </div>
      </TableCell>
      <TableCell className="text-muted-foreground max-w-56 truncate">{user.email}</TableCell>
      <TableCell className="text-muted-foreground">{location}</TableCell>
      <TableCell className="text-right tabular-nums">{ageFrom(user.birthdate)}</TableCell>
      <TableCell>
        {user.isBanned ? (
          <Badge variant="destructive">Banned</Badge>
        ) : (
          <Badge variant="secondary">Active</Badge>
        )}
      </TableCell>
      <TableCell className="text-right">
        <Button
          size="sm"
          variant={user.isBanned ? "outline" : "destructive"}
          onClick={onModerate}
        >
          {user.isBanned ? "Unban" : "Ban"}
        </Button>
      </TableCell>
    </TableRow>
  );
}

function SkeletonRows() {
  return (
    <>
      {Array.from({ length: 5 }, (_, index) => (
        <TableRow key={index}>
          <TableCell>
            <div className="flex items-center gap-2.5">
              <Skeleton className="size-8 rounded-full" />
              <Skeleton className="h-4 w-28" />
            </div>
          </TableCell>
          <TableCell>
            <Skeleton className="h-4 w-40" />
          </TableCell>
          <TableCell>
            <Skeleton className="h-4 w-32" />
          </TableCell>
          <TableCell>
            <Skeleton className="ml-auto h-4 w-8" />
          </TableCell>
          <TableCell>
            <Skeleton className="h-5 w-16 rounded-full" />
          </TableCell>
          <TableCell>
            <Skeleton className="ml-auto h-8 w-16" />
          </TableCell>
        </TableRow>
      ))}
    </>
  );
}
