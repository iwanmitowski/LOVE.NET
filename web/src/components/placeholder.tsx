import { Construction } from "lucide-react";

/** Temporary stub for feature pages that are being built in parallel. */
export default function Placeholder({ title }: { title: string }) {
  return (
    <div className="text-muted-foreground grid h-full place-items-center p-8 text-center">
      <div className="space-y-2">
        <Construction className="mx-auto size-8" />
        <p className="font-medium">{title}</p>
        <p className="text-sm">Coming right up.</p>
      </div>
    </div>
  );
}
