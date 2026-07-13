import { ImagePlus, Star, X } from "lucide-react";
import { cn } from "@/lib/utils";

export interface PhotoItem {
  id: string;
  file: File;
  /** Object URL for the thumbnail preview (owner revokes it). */
  url: string;
}

interface PhotoPickerProps {
  photos: PhotoItem[];
  /** Photo currently marked as the profile picture. */
  profileId: string | null;
  onAdd: (files: File[]) => void;
  onRemove: (id: string) => void;
  onChooseProfile: (id: string) => void;
}

/** Multi-photo upload with previews, a star to pick the profile picture, and remove buttons. */
export default function PhotoPicker({
  photos,
  profileId,
  onAdd,
  onRemove,
  onChooseProfile,
}: PhotoPickerProps) {
  return (
    <div className="space-y-3">
      <label
        className={cn(
          "border-input hover:bg-accent/50 flex cursor-pointer flex-col items-center",
          "justify-center gap-1 rounded-lg border border-dashed p-6 text-center transition-colors",
        )}
      >
        <ImagePlus className="text-muted-foreground size-6" />
        <span className="text-sm font-medium">Add photos</span>
        <span className="text-muted-foreground text-xs">JPG or PNG — you can pick several</span>
        <input
          type="file"
          multiple
          accept=".jpg,.jpeg,.png"
          className="sr-only"
          onChange={(event) => {
            const files = event.target.files;
            if (files && files.length > 0) onAdd(Array.from(files));
            event.target.value = "";
          }}
        />
      </label>

      {photos.length > 0 && (
        <>
          <div className="grid grid-cols-3 gap-2">
            {photos.map((photo) => {
              const isProfile = photo.id === profileId;
              return (
                <div
                  key={photo.id}
                  className={cn(
                    "group relative aspect-square overflow-hidden rounded-lg border",
                    isProfile && "ring-primary ring-2",
                  )}
                >
                  <img
                    src={photo.url}
                    alt={photo.file.name}
                    className="size-full object-cover"
                  />
                  <button
                    type="button"
                    title={isProfile ? "Profile picture" : "Use as profile picture"}
                    aria-label={isProfile ? "Profile picture" : "Use as profile picture"}
                    onClick={() => onChooseProfile(photo.id)}
                    className={cn(
                      "absolute top-1 left-1 grid size-6 place-items-center rounded-full",
                      "bg-black/50 transition-colors hover:bg-black/70",
                      isProfile ? "text-amber-400" : "text-white",
                    )}
                  >
                    <Star className="size-3.5" fill={isProfile ? "currentColor" : "none"} />
                  </button>
                  <button
                    type="button"
                    title="Remove photo"
                    aria-label="Remove photo"
                    onClick={() => onRemove(photo.id)}
                    className={cn(
                      "absolute top-1 right-1 grid size-6 place-items-center rounded-full",
                      "bg-black/50 text-white transition-colors hover:bg-black/70",
                    )}
                  >
                    <X className="size-3.5" />
                  </button>
                </div>
              );
            })}
          </div>
          <p className="text-muted-foreground text-xs">
            The starred photo becomes your profile picture.
          </p>
        </>
      )}
    </div>
  );
}
