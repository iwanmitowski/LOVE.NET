import { Heart } from "lucide-react";
import type { ReactNode } from "react";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { cn } from "@/lib/utils";

interface AuthCardProps {
  title: string;
  subtitle: string;
  /** Width override, e.g. "max-w-md" for the wider register wizard. */
  widthClass?: string;
  children: ReactNode;
}

/** Centered auth page shell matching the login page's look. */
export default function AuthCard({
  title,
  subtitle,
  widthClass = "max-w-sm",
  children,
}: AuthCardProps) {
  return (
    <div className="grid min-h-dvh place-items-center p-4">
      <Card className={cn("w-full", widthClass)}>
        <CardHeader className="items-center text-center">
          <span className="gradient-brand mx-auto grid size-12 place-items-center rounded-2xl">
            <Heart className="size-6 text-white" fill="currentColor" />
          </span>
          <h1 className="text-gradient-brand text-2xl font-extrabold">{title}</h1>
          <p className="text-muted-foreground text-sm">{subtitle}</p>
        </CardHeader>
        <CardContent>{children}</CardContent>
      </Card>
    </div>
  );
}
