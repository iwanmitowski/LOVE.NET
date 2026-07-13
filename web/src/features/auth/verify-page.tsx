import { useMutation, useQuery } from "@tanstack/react-query";
import { BadgeCheck, Loader2, MailCheck } from "lucide-react";
import { Link, useSearchParams } from "react-router";
import { resendVerificationEmail, verifyEmail } from "@/api/identity";
import { Button } from "@/components/ui/button";
import AuthCard from "@/features/auth/auth-card";
import { errorMessages } from "@/lib/errors";

export default function VerifyPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token");
  const email = searchParams.get("email") ?? "";

  const verifyQuery = useQuery({
    queryKey: ["verify-email", token, email],
    queryFn: () => verifyEmail(token ?? "", email),
    enabled: token !== null,
    retry: false,
  });

  const resend = useMutation({ mutationFn: () => resendVerificationEmail(email) });

  const resendBlock = (
    <div className="w-full space-y-2">
      {resend.isError && (
        <div className="text-destructive space-y-0.5 text-sm">
          {errorMessages(resend.error).map((line) => (
            <p key={line}>{line}</p>
          ))}
        </div>
      )}
      {resend.isSuccess && <p className="text-muted-foreground text-sm">{resend.data}</p>}
      <Button
        variant="outline"
        className="w-full"
        disabled={!email || resend.isPending}
        onClick={() => resend.mutate()}
      >
        {resend.isPending ? "Sending…" : "Resend email"}
      </Button>
    </div>
  );

  let content;
  if (token !== null && verifyQuery.isPending) {
    content = (
      <>
        <Loader2 className="text-muted-foreground size-8 animate-spin" />
        <p className="text-muted-foreground text-sm">Confirming your email…</p>
      </>
    );
  } else if (token !== null && verifyQuery.isSuccess) {
    content = (
      <>
        <BadgeCheck className="size-10 text-emerald-500" />
        <p className="text-sm font-medium">Email confirmed — you can now login.</p>
        <Button asChild className="w-full">
          <Link to="/login">Log in</Link>
        </Button>
      </>
    );
  } else if (token !== null) {
    content = (
      <>
        <div className="text-destructive space-y-0.5 text-sm">
          {errorMessages(verifyQuery.error).map((line) => (
            <p key={line}>{line}</p>
          ))}
        </div>
        {resendBlock}
      </>
    );
  } else {
    content = (
      <>
        <MailCheck className="text-muted-foreground size-10" />
        <p className="text-sm font-medium">Check your inbox</p>
        <p className="text-muted-foreground text-sm">
          We sent a verification link to{" "}
          <span className="text-foreground font-medium">{email || "your email address"}</span>
          . Click it to activate your account.
        </p>
        {resendBlock}
      </>
    );
  }

  return (
    <AuthCard title="Verify your email" subtitle="One last step before the fun starts">
      <div className="flex flex-col items-center gap-4 text-center">
        {content}
        <p className="text-muted-foreground text-sm">
          Already verified?{" "}
          <Link className="text-primary hover:underline" to="/login">
            Log in
          </Link>
        </p>
      </div>
    </AuthCard>
  );
}
