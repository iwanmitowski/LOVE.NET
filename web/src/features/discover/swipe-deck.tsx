import { useCallback, useImperativeHandle, useRef, type Ref } from "react";
import { Heart, X } from "lucide-react";
import {
  AnimatePresence,
  animate,
  motion,
  useMotionValue,
  useTransform,
  type PanInfo,
} from "motion/react";
import type { UserCard } from "@/api/types";
import { Button } from "@/components/ui/button";
import { distanceInKms } from "@/lib/distance";
import { cn } from "@/lib/utils";
import SwipeCard from "@/features/discover/swipe-card";
import type { GeoLocation } from "@/stores/auth";

export type SwipeDirection = "left" | "right";

const VISIBLE_CARDS = 3;
const SWIPE_OFFSET_THRESHOLD = 120;
const SWIPE_VELOCITY_THRESHOLD = 500;
const PROGRAMMATIC_FLING_VELOCITY = 900;

interface SwipeDeckProps {
  cards: UserCard[];
  myLocation: GeoLocation | null;
  /** Disables the action buttons while a like request is in flight. */
  likePending: boolean;
  onSwipe: (card: UserCard, direction: SwipeDirection) => void;
}

/** Stacked deck: top card is draggable, the next two peek from behind; action bar below. */
export default function SwipeDeck({ cards, myLocation, likePending, onSwipe }: SwipeDeckProps) {
  const topCardRef = useRef<DeckCardHandle>(null);
  const visible = cards.slice(0, VISIBLE_CARDS);
  const actionsDisabled = likePending || visible.length === 0;

  return (
    <div className="flex h-full flex-col gap-5">
      <div className="relative min-h-0 flex-1">
        <AnimatePresence initial={false}>
          {visible.map((card, index) => (
            <DeckCard
              key={card.id}
              ref={index === 0 ? topCardRef : undefined}
              card={card}
              index={index}
              stackSize={visible.length}
              isTop={index === 0}
              distanceKm={
                myLocation
                  ? distanceInKms(
                      myLocation.latitude,
                      myLocation.longitude,
                      card.latitude,
                      card.longitude,
                    )
                  : null
              }
              onCommit={onSwipe}
            />
          ))}
        </AnimatePresence>
      </div>

      <div className="flex items-center justify-center gap-6">
        <Button
          variant="outline"
          size="icon"
          aria-label="Pass"
          className="size-14 rounded-full border-2 text-rose-500 shadow-md hover:bg-rose-500/10 hover:text-rose-500"
          disabled={actionsDisabled}
          onClick={() => topCardRef.current?.swipe("left")}
        >
          <X className="size-7" />
        </Button>
        <Button
          size="icon"
          aria-label="Like"
          className="gradient-brand size-16 rounded-full border-0 text-white shadow-lg hover:opacity-90"
          disabled={actionsDisabled}
          onClick={() => topCardRef.current?.swipe("right")}
        >
          <Heart className="size-8" fill="currentColor" />
        </Button>
      </div>
    </div>
  );
}

interface DeckCardHandle {
  swipe: (direction: SwipeDirection) => void;
}

interface DeckCardProps {
  card: UserCard;
  index: number;
  stackSize: number;
  isTop: boolean;
  distanceKm: number | null;
  onCommit: (card: UserCard, direction: SwipeDirection) => void;
  ref?: Ref<DeckCardHandle>;
}

function DeckCard({ card, index, stackSize, isTop, distanceKm, onCommit, ref }: DeckCardProps) {
  const x = useMotionValue(0);
  const rotate = useTransform(x, [-200, 200], [-18, 18]);
  const likeOpacity = useTransform(x, [40, 140], [0, 1]);
  // Same as the spec's [-40, -140] → [0, 1], written with an ascending input range.
  const nopeOpacity = useTransform(x, [-140, -40], [1, 0]);

  const committed = useRef(false);
  const suppressClick = useRef(false);

  const fling = useCallback(
    (direction: 1 | -1, velocity: number = direction * PROGRAMMATIC_FLING_VELOCITY) => {
      if (committed.current) return;
      committed.current = true;
      const target = direction * window.innerWidth;
      void animate(x, target, {
        type: "spring",
        stiffness: 160,
        damping: 26,
        velocity,
        restDelta: 1,
      }).then(() => onCommit(card, direction > 0 ? "right" : "left"));
    },
    [card, onCommit, x],
  );

  useImperativeHandle(ref, () => ({ swipe: (dir) => fling(dir === "right" ? 1 : -1) }), [fling]);

  function handleDragEnd(_: unknown, info: PanInfo) {
    // The click that follows a drag release would step the photo carousel; swallow it.
    suppressClick.current = true;
    requestAnimationFrame(() => {
      suppressClick.current = false;
    });

    const { offset, velocity } = info;
    const passedOffset = Math.abs(offset.x) > SWIPE_OFFSET_THRESHOLD;
    if (passedOffset || Math.abs(velocity.x) > SWIPE_VELOCITY_THRESHOLD) {
      const direction = (passedOffset ? offset.x : velocity.x) > 0 ? 1 : -1;
      fling(direction, velocity.x);
    }
    // Otherwise the all-zero dragConstraints spring the card back automatically.
  }

  return (
    <motion.div
      className={cn("absolute inset-0", isTop ? "touch-none" : "pointer-events-none")}
      style={{ x, rotate, zIndex: stackSize - index }}
      initial={{ scale: 0.9, y: 28, opacity: index === 0 ? 1 : 0 }}
      animate={{ scale: 1 - index * 0.05, y: index * 14, opacity: 1 }}
      exit={{ opacity: 0, transition: { duration: 0.12 } }}
      transition={{ type: "spring", stiffness: 300, damping: 30 }}
      drag={isTop}
      dragConstraints={{ left: 0, right: 0, top: 0, bottom: 0 }}
      dragElastic={0.9}
      onDragEnd={handleDragEnd}
      onClickCapture={(event) => {
        if (suppressClick.current) {
          event.preventDefault();
          event.stopPropagation();
        }
      }}
    >
      <SwipeCard
        card={card}
        distanceKm={distanceKm}
        likeOpacity={isTop ? likeOpacity : undefined}
        nopeOpacity={isTop ? nopeOpacity : undefined}
      />
    </motion.div>
  );
}
