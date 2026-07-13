import { useQueryClient, type InfiniteData } from "@tanstack/react-query";
import { ImagePlus, Send, X } from "lucide-react";
import { useEffect, useRef, useState, type ChangeEvent, type ClipboardEvent, type FormEvent } from "react";
import { toast } from "sonner";
import { uploadChatImage } from "@/api/chat";
import type { ChatMessage, ChatPage } from "@/api/types";
import {
  Attachment,
  AttachmentAction,
  AttachmentActions,
  AttachmentContent,
  AttachmentDescription,
  AttachmentMedia,
  AttachmentTitle,
} from "@/components/ui/attachment";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useRealtime } from "@/realtime/connection";
import { useAuthStore } from "@/stores/auth";

const TYPING_THROTTLE_MS = 2_000;

interface PendingImage {
  file: File;
  previewUrl: string;
}

/**
 * Message composer: text + image (paste or attach). Sends are optimistic —
 * a pending bubble goes into the ['messages', roomId] cache immediately and
 * the RealtimeProvider swaps it for the hub echo (or we remove it on failure).
 */
export default function Composer({ roomId, onSent }: { roomId: string; onSent?: () => void }) {
  const realtime = useRealtime();
  const queryClient = useQueryClient();
  const user = useAuthStore((s) => s.user);

  const [text, setText] = useState("");
  const [image, setImage] = useState<PendingImage | null>(null);
  const [busy, setBusy] = useState(false);
  const lastTypingAt = useRef(0);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Release the preview object URL whenever the image is replaced or cleared.
  useEffect(() => {
    return () => {
      if (image) URL.revokeObjectURL(image.previewUrl);
    };
  }, [image]);

  function onTextChange(e: ChangeEvent<HTMLInputElement>) {
    setText(e.target.value);
    const now = Date.now();
    if (now - lastTypingAt.current >= TYPING_THROTTLE_MS) {
      lastTypingAt.current = now;
      realtime.typing(roomId);
    }
  }

  function pickImage(file: File | null | undefined) {
    if (!file || !file.type.startsWith("image/")) return;
    setImage({ file, previewUrl: URL.createObjectURL(file) });
  }

  function onPaste(e: ClipboardEvent<HTMLInputElement>) {
    const item = Array.from(e.clipboardData.items).find((i) => i.type.startsWith("image/"));
    if (item) pickImage(item.getAsFile());
  }

  function appendOptimistic(body: { text?: string | null; imageUrl?: string | null }): ChatMessage {
    const temp: ChatMessage = {
      id: `temp-${crypto.randomUUID()}`,
      roomId,
      userId: user?.id ?? "",
      text: body.text ?? null,
      profilePicture: user?.profilePicture ?? null,
      imageUrl: body.imageUrl ?? null,
      createdOn: new Date().toISOString(),
      pending: true,
    };
    queryClient.setQueryData<InfiniteData<ChatPage>>(["messages", roomId], (data) => {
      if (!data || data.pages.length === 0) return data;
      const [first, ...rest] = data.pages;
      return {
        ...data,
        pages: [
          { messages: [temp, ...first.messages], totalMessages: first.totalMessages + 1 },
          ...rest,
        ],
      };
    });
    return temp;
  }

  function removeOptimistic(id: string) {
    queryClient.setQueryData<InfiniteData<ChatPage>>(["messages", roomId], (data) => {
      if (!data) return data;
      return {
        ...data,
        pages: data.pages.map((page) =>
          page.messages.some((m) => m.id === id)
            ? {
                messages: page.messages.filter((m) => m.id !== id),
                totalMessages: page.totalMessages - 1,
              }
            : page,
        ),
      };
    });
  }

  async function deliver(body: { text?: string | null; imageUrl?: string | null }) {
    const temp = appendOptimistic(body);
    onSent?.();
    try {
      await realtime.sendMessage({ roomId, ...body, profilePicture: user?.profilePicture ?? null });
    } catch {
      removeOptimistic(temp.id);
      toast.error("Message failed to send. Try again.");
    }
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    if (busy) return;

    const trimmed = text.trim();
    const picked = image;
    if (!trimmed && !picked) return;

    setText("");
    setBusy(true);
    try {
      // Text and image travel as two separate messages (old-app contract).
      if (trimmed) await deliver({ text: trimmed });
      if (picked) {
        try {
          const imageUrl = await uploadChatImage(picked.file);
          setImage(null);
          await deliver({ imageUrl });
        } catch {
          toast.error("Couldn't upload the image.");
        }
      }
    } finally {
      setBusy(false);
    }
  }

  return (
    <form onSubmit={(e) => void onSubmit(e)} className="shrink-0 border-t p-3">
      {image && (
        <div className="mb-2">
          <Attachment size="sm" state={busy ? "uploading" : "idle"}>
            <AttachmentMedia variant="image">
              <img src={image.previewUrl} alt="Attached preview" />
            </AttachmentMedia>
            <AttachmentContent>
              <AttachmentTitle>{image.file.name || "Pasted image"}</AttachmentTitle>
              <AttachmentDescription>
                {busy ? "Uploading…" : "Ready to send"}
              </AttachmentDescription>
            </AttachmentContent>
            <AttachmentActions>
              <AttachmentAction
                aria-label="Remove image"
                onClick={() => setImage(null)}
                disabled={busy}
              >
                <X />
              </AttachmentAction>
            </AttachmentActions>
          </Attachment>
        </div>
      )}
      <div className="flex items-center gap-2">
        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          className="hidden"
          onChange={(e) => {
            pickImage(e.target.files?.[0]);
            e.target.value = "";
          }}
        />
        <Button
          type="button"
          variant="ghost"
          size="icon"
          aria-label="Attach an image"
          onClick={() => fileInputRef.current?.click()}
        >
          <ImagePlus className="size-5" />
        </Button>
        <Input
          value={text}
          onChange={onTextChange}
          onPaste={onPaste}
          placeholder="Type a message"
          aria-label="Message"
          autoComplete="off"
          className="flex-1 rounded-full"
        />
        <Button
          type="submit"
          size="icon"
          aria-label="Send message"
          disabled={busy || (!text.trim() && !image)}
          className="gradient-brand rounded-full border-0 text-white hover:opacity-90"
        >
          <Send className="size-4" />
        </Button>
      </div>
    </form>
  );
}
