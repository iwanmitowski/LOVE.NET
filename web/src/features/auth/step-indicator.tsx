import { cn } from "@/lib/utils";

interface StepIndicatorProps {
  /** Zero-based current step. */
  step: number;
  labels: readonly string[];
}

/** Segmented wizard progress: one bar per step plus a "Step x of y" caption. */
export default function StepIndicator({ step, labels }: StepIndicatorProps) {
  return (
    <div className="space-y-2">
      <div className="flex gap-1.5" role="presentation">
        {labels.map((label, index) => (
          <div
            key={label}
            className={cn(
              "h-1.5 flex-1 rounded-full transition-colors",
              index <= step ? "gradient-brand" : "bg-muted",
            )}
          />
        ))}
      </div>
      <p className="text-muted-foreground text-center text-xs">
        Step {step + 1} of {labels.length} —{" "}
        <span className="text-foreground font-medium">{labels[step]}</span>
      </p>
    </div>
  );
}
