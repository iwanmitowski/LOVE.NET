import { ImagePlus, Star, Trash2, X } from "lucide-react";
import type { ImageModel } from "@/api/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

export interface NewPhoto {
  file: File;
  /** Object URL used for the preview; revoke it when the photo is dropped. */
  url: string;
}

interface PhotoManagerProps {
  images: ImageModel[];
  newPhotos: NewPhoto[];
  onMakeProfilePicture: (id: number) => void;
  onRemoveImage: (id: number) => void;
  onAddFiles: (files: FileList | null) => void;
  onRemoveNewPhoto: (url: string) => void;
}

export default function PhotoManager({
  images,
  newPhotos,
  onMakeProfilePicture,
  onRemoveImage,
  onAddFiles,
  onRemoveNewPhoto,
}: PhotoManagerProps) {
  return (
    <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-2 xl:grid-cols-3">
      {images.map((image) => (
        <PhotoTile
          key={image.id}
          image={image}
          onMakeProfilePicture={() => onMakeProfilePicture(image.id)}
          onRemove={() => onRemoveImage(image.id)}
        />
      ))}

      {newPhotos.map((photo) => (
        <div key={photo.url} className="relative aspect-square overflow-hidden rounded-xl border">
          <img src={photo.url} alt="New photo preview" className="size-full object-cover" />
          <Badge variant="secondary" className="absolute top-2 left-2">
            New
          </Badge>
          <Button
            type="button"
            variant="secondary"
            size="icon-xs"
            className="absolute top-2 right-2"
            aria-label="Remove new photo"
            onClick={() => onRemoveNewPhoto(photo.url)}
          >
            <X />
          </Button>
        </div>
      ))}

      <label className="border-muted-foreground/40 text-muted-foreground hover:border-primary hover:text-primary grid aspect-square cursor-pointer place-items-center rounded-xl border-2 border-dashed transition-colors">
        <span className="flex flex-col items-center gap-1.5 text-sm font-medium">
          <ImagePlus className="size-6" />
          Add photos
        </span>
        <input
          type="file"
          className="sr-only"
          multiple
          accept=".jpg,.jpeg,.png"
          onChange={(e) => {
            onAddFiles(e.target.files);
            e.target.value = "";
          }}
        />
      </label>
    </div>
  );
}

function PhotoTile({
  image,
  onMakeProfilePicture,
  onRemove,
}: {
  image: ImageModel;
  onMakeProfilePicture: () => void;
  onRemove: () => void;
}) {
  return (
    <div className="group relative aspect-square overflow-hidden rounded-xl border">
      <img src={image.url} alt="Profile photo" className="size-full object-cover" />

      {image.isProfilePicture ? (
        <Badge className="gradient-brand absolute top-2 left-2 border-0 text-white">
          <Star className="size-3" fill="currentColor" /> Profile
        </Badge>
      ) : (
        // Visible on touch; revealed on hover/focus for pointer devices.
        <div className="absolute inset-0 flex items-center justify-center gap-2 bg-black/40 transition-opacity sm:opacity-0 sm:group-focus-within:opacity-100 sm:group-hover:opacity-100">
          <Button
            type="button"
            variant="secondary"
            size="icon-sm"
            aria-label="Make profile picture"
            onClick={onMakeProfilePicture}
          >
            <Star />
          </Button>
          <Button
            type="button"
            variant="destructive"
            size="icon-sm"
            aria-label="Remove photo"
            onClick={onRemove}
          >
            <Trash2 />
          </Button>
        </div>
      )}
    </div>
  );
}
