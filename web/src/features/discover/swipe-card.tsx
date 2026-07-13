import { useMemo, useState } from "react";
import { MapPin } from "lucide-react";
import { motion, type MotionValue } from "motion/react";
import type { UserCard } from "@/api/types";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

interface SwipeCardProps {
  card: UserCard;
  /** Kilometers from the logged-in user, or null when their location is unknown. */
  distanceKm: number | null;
  /** Stamp opacities driven by the drag x motion value; only passed for the top card. */
  likeOpacity?: MotionValue<number>;
  nopeOpacity?: MotionValue<number>;
}

/** Presentational swipe card: full-bleed photo carousel, info overlay, LIKE/NOPE stamps. */
export default function SwipeCard({ card, distanceKm, likeOpacity, nopeOpacity }: SwipeCardProps) {
  const photos = useMemo(
    () => [...card.images].sort((a, b) => Number(b.isProfilePicture) - Number(a.isProfilePicture)),
    [card.images],
  );
  const [photoIndex, setPhotoIndex] = useState(0);
  const photo = photos[Math.min(photoIndex, photos.length - 1)];

  const caption = [
    [card.cityName, card.countryName].filter(Boolean).join(", "),
    card.genderName,
  ]
    .filter(Boolean)
    .join(" · ");

  return (
    <div className="bg-card relative h-full w-full overflow-hidden rounded-3xl border shadow-xl select-none">
      {photo ? (
        <img
          src={photo.url}
          alt={card.userName}
          draggable={false}
          className="absolute inset-0 h-full w-full object-cover"
        />
      ) : (
        <div className="gradient-brand absolute inset-0 grid place-items-center">
          <span className="text-7xl font-extrabold text-white/90">
            {card.userName[0]?.toUpperCase()}
          </span>
        </div>
      )}

      {/* Tap left/right halves to step through photos. */}
      {photos.length > 1 && (
        <>
          <button
            type="button"
            tabIndex={-1}
            aria-label="Previous photo"
            className="absolute inset-y-0 left-0 w-1/2 cursor-default outline-none"
            onClick={() => setPhotoIndex((i) => Math.max(0, i - 1))}
          />
          <button
            type="button"
            tabIndex={-1}
            aria-label="Next photo"
            className="absolute inset-y-0 right-0 w-1/2 cursor-default outline-none"
            onClick={() => setPhotoIndex((i) => Math.min(photos.length - 1, i + 1))}
          />
          <div className="absolute inset-x-3 top-3 flex gap-1.5">
            {photos.map((p, i) => (
              <span
                key={p.id}
                className={cn(
                  "h-1 flex-1 rounded-full transition-colors",
                  i === photoIndex ? "bg-white" : "bg-white/40",
                )}
              />
            ))}
          </div>
        </>
      )}

      {likeOpacity && (
        <motion.span
          style={{ opacity: likeOpacity }}
          className="pointer-events-none absolute top-8 left-6 -rotate-12 rounded-lg border-4 border-emerald-400 px-3 py-1 text-4xl font-extrabold tracking-widest text-emerald-400"
        >
          LIKE
        </motion.span>
      )}
      {nopeOpacity && (
        <motion.span
          style={{ opacity: nopeOpacity }}
          className="pointer-events-none absolute top-8 right-6 rotate-12 rounded-lg border-4 border-rose-500 px-3 py-1 text-4xl font-extrabold tracking-widest text-rose-500"
        >
          NOPE
        </motion.span>
      )}

      <div className="pointer-events-none absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/85 via-black/45 to-transparent p-5 pt-16 text-white">
        <div className="flex items-end justify-between gap-2">
          <h2 className="text-2xl font-bold">
            {card.userName}
            <span className="ml-2 text-xl font-medium text-white/85">{card.age}</span>
          </h2>
          {distanceKm !== null && (
            <Badge className="mb-0.5 shrink-0 border-0 bg-white/20 text-white backdrop-blur-sm">
              <MapPin /> {distanceKm} km away
            </Badge>
          )}
        </div>
        {card.bio && <p className="mt-1 line-clamp-2 text-sm text-white/85">{card.bio}</p>}
        {caption && <p className="mt-1 text-xs text-white/60">{caption}</p>}
      </div>
    </div>
  );
}
