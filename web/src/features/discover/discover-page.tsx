import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { HeartOff, RotateCw } from "lucide-react";
import { AnimatePresence } from "motion/react";
import { Navigate } from "react-router";
import { getUsersToSwipe, likeUser } from "@/api/dating";
import type { UserCard } from "@/api/types";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { distanceInKms } from "@/lib/distance";
import MatchOverlay from "@/features/discover/match-overlay";
import PreferencesSheet from "@/features/discover/preferences-sheet";
import SwipeDeck, { type SwipeDirection } from "@/features/discover/swipe-deck";
import { useAuthStore } from "@/stores/auth";
import { usePreferencesStore } from "@/stores/preferences";

export default function DiscoverPage() {
  const user = useAuthStore((s) => s.user);
  const location = useAuthStore((s) => s.location);
  const maxAge = usePreferencesStore((s) => s.maxAge);
  const maxDistance = usePreferencesStore((s) => s.maxDistance);
  const aroundTheWorld = usePreferencesStore((s) => s.aroundTheWorld);
  const gender = usePreferencesStore((s) => s.gender);

  const queryClient = useQueryClient();
  const deckQuery = useQuery({
    queryKey: ["deck"],
    queryFn: getUsersToSwipe,
    enabled: !user?.isAdmin,
  });

  // Swiped cards disappear locally without refetching the deck.
  const [removedIds, setRemovedIds] = useState<ReadonlySet<string>>(new Set());
  const [match, setMatch] = useState<UserCard | null>(null);

  const likeMutation = useMutation({
    mutationFn: likeUser,
    onSuccess: (response) => {
      if (response.isMatch && response.user) setMatch(response.user);
    },
  });

  const cards = useMemo(
    () =>
      (deckQuery.data ?? []).filter((candidate) => {
        if (removedIds.has(candidate.id)) return false;
        if (candidate.age > maxAge) return false;
        if (gender !== -1 && candidate.genderId !== gender) return false;
        if (!aroundTheWorld && location) {
          const km = distanceInKms(
            location.latitude,
            location.longitude,
            candidate.latitude,
            candidate.longitude,
          );
          return km <= maxDistance;
        }
        return true;
      }),
    [deckQuery.data, removedIds, maxAge, gender, aroundTheWorld, location, maxDistance],
  );

  if (user?.isAdmin) {
    return <Navigate to="/admin" replace />;
  }

  function handleSwipe(card: UserCard, direction: SwipeDirection) {
    setRemovedIds((prev) => new Set(prev).add(card.id));
    if (direction === "right") likeMutation.mutate(card.id);
  }

  function refreshDeck() {
    setRemovedIds(new Set());
    void queryClient.invalidateQueries({ queryKey: ["deck"] });
  }

  return (
    <div className="mx-auto flex h-full w-full max-w-md flex-col px-4 py-4">
      <header className="mb-4 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-extrabold tracking-tight">Discover</h1>
          <p className="text-muted-foreground text-sm">Find your next spark</p>
        </div>
        <PreferencesSheet />
      </header>

      <div className="min-h-0 flex-1">
        {deckQuery.isPending ? (
          <DeckSkeleton />
        ) : cards.length === 0 ? (
          <EmptyDeck onRefresh={refreshDeck} refreshing={deckQuery.isFetching} />
        ) : (
          <SwipeDeck
            cards={cards}
            myLocation={location}
            likePending={likeMutation.isPending}
            onSwipe={handleSwipe}
          />
        )}
      </div>

      <AnimatePresence>
        {match && <MatchOverlay user={match} onDismiss={() => setMatch(null)} />}
      </AnimatePresence>
    </div>
  );
}

function DeckSkeleton() {
  return (
    <div className="flex h-full flex-col gap-5">
      <Skeleton className="min-h-0 w-full flex-1 rounded-3xl" />
      <div className="flex items-center justify-center gap-6">
        <Skeleton className="size-14 rounded-full" />
        <Skeleton className="size-16 rounded-full" />
      </div>
    </div>
  );
}

function EmptyDeck({ onRefresh, refreshing }: { onRefresh: () => void; refreshing: boolean }) {
  return (
    <Card className="grid h-full place-items-center rounded-3xl">
      <CardContent className="flex flex-col items-center gap-4 text-center">
        <span className="bg-muted grid size-14 place-items-center rounded-full">
          <HeartOff className="text-muted-foreground size-7" />
        </span>
        <div className="space-y-1">
          <h2 className="font-semibold">No more profiles</h2>
          <p className="text-muted-foreground text-sm">
            You have seen everyone nearby — check back later.
          </p>
        </div>
        <Button variant="outline" onClick={onRefresh} disabled={refreshing}>
          <RotateCw className={refreshing ? "animate-spin" : undefined} />
          Refresh
        </Button>
      </CardContent>
    </Card>
  );
}
