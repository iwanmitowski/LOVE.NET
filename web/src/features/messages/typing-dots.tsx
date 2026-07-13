/** Animated three-dot "peer is typing" bubble. */
export default function TypingDots() {
  return (
    <div
      className="bg-muted flex w-fit items-center gap-1 rounded-xl px-3 py-2.5"
      role="status"
      aria-label="Typing"
    >
      <span className="bg-muted-foreground size-1.5 animate-bounce rounded-full" />
      <span className="bg-muted-foreground size-1.5 animate-bounce rounded-full [animation-delay:150ms]" />
      <span className="bg-muted-foreground size-1.5 animate-bounce rounded-full [animation-delay:300ms]" />
    </div>
  );
}
