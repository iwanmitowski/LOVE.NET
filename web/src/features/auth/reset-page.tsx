import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { BadgeCheck, MailCheck } from "lucide-react";
import { useForm } from "react-hook-form";
import { Link, useSearchParams } from "react-router";
import { z } from "zod";
import { resetPassword, sendResetPasswordLink } from "@/api/identity";
import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import AuthCard from "@/features/auth/auth-card";
import { validation } from "@/lib/constants";
import { errorMessages } from "@/lib/errors";

// Dual mode like the old app: without a token we email a reset link,
// with a token (+ email) we set the new password.
export default function ResetPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token");
  const email = searchParams.get("email") ?? "";

  return token ? <NewPasswordForm token={token} email={email} /> : <RequestLinkForm />;
}

const requestSchema = z.object({
  email: z.email("Invalid email"),
});

type RequestValues = z.infer<typeof requestSchema>;

function RequestLinkForm() {
  const form = useForm<RequestValues>({
    resolver: zodResolver(requestSchema),
    defaultValues: { email: "" },
  });

  const mutation = useMutation({
    mutationFn: (values: RequestValues) => sendResetPasswordLink(values.email),
  });

  if (mutation.isSuccess) {
    return (
      <AuthCard title="Check your email" subtitle="The reset link is on its way">
        <div className="flex flex-col items-center gap-4 text-center">
          <MailCheck className="text-muted-foreground size-10" />
          <p className="text-muted-foreground text-sm">
            We sent a password reset link to{" "}
            <span className="text-foreground font-medium">
              {form.getValues("email")}
            </span>
            . Follow it to choose a new password.
          </p>
          <Button
            variant="outline"
            className="w-full"
            onClick={() => mutation.reset()}
          >
            Use a different email
          </Button>
          <BackToLogin />
        </div>
      </AuthCard>
    );
  }

  return (
    <AuthCard title="Reset password" subtitle="We'll email you a reset link">
      <Form {...form}>
        <form
          className="space-y-4"
          onSubmit={form.handleSubmit((values) => mutation.mutate(values))}
        >
          <FormField
            control={form.control}
            name="email"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Email</FormLabel>
                <FormControl>
                  <Input type="email" autoComplete="email" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {mutation.isError && <ErrorLines error={mutation.error} />}

          <Button type="submit" className="w-full" disabled={mutation.isPending}>
            {mutation.isPending ? "Sending…" : "Send reset link"}
          </Button>
        </form>
      </Form>
      <div className="mt-4 text-center">
        <BackToLogin />
      </div>
    </AuthCard>
  );
}

const passwordSchema = z
  .object({
    password: z.string().min(validation.PASSWORD_MIN_LENGTH, "Password is too short"),
    confirmPassword: z.string().min(1, "Confirm your password"),
  })
  .refine((values) => values.password === values.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

type PasswordValues = z.infer<typeof passwordSchema>;

function NewPasswordForm({ token, email }: { token: string; email: string }) {
  const form = useForm<PasswordValues>({
    resolver: zodResolver(passwordSchema),
    defaultValues: { password: "", confirmPassword: "" },
  });

  const mutation = useMutation({
    mutationFn: (values: PasswordValues) =>
      resetPassword({
        token,
        email,
        password: values.password,
        confirmPassword: values.confirmPassword,
      }),
  });

  if (mutation.isSuccess) {
    return (
      <AuthCard title="Password changed" subtitle="You're all set">
        <div className="flex flex-col items-center gap-4 text-center">
          <BadgeCheck className="size-10 text-emerald-500" />
          <p className="text-muted-foreground text-sm">
            {mutation.data || "Your password was reset — log in with the new one."}
          </p>
          <Button asChild className="w-full">
            <Link to="/login">Log in</Link>
          </Button>
        </div>
      </AuthCard>
    );
  }

  return (
    <AuthCard title="Reset password" subtitle="Choose a new password">
      <Form {...form}>
        <form
          className="space-y-4"
          onSubmit={form.handleSubmit((values) => mutation.mutate(values))}
        >
          <FormField
            control={form.control}
            name="password"
            render={({ field }) => (
              <FormItem>
                <FormLabel>New password</FormLabel>
                <FormControl>
                  <Input type="password" autoComplete="new-password" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="confirmPassword"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Confirm password</FormLabel>
                <FormControl>
                  <Input type="password" autoComplete="new-password" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {mutation.isError && <ErrorLines error={mutation.error} />}

          <Button type="submit" className="w-full" disabled={mutation.isPending}>
            {mutation.isPending ? "Resetting…" : "Reset password"}
          </Button>
        </form>
      </Form>
      <div className="mt-4 text-center">
        <BackToLogin />
      </div>
    </AuthCard>
  );
}

function ErrorLines({ error }: { error: unknown }) {
  return (
    <div className="text-destructive space-y-0.5 text-sm">
      {errorMessages(error).map((line) => (
        <p key={line}>{line}</p>
      ))}
    </div>
  );
}

function BackToLogin() {
  return (
    <p className="text-muted-foreground text-sm">
      Remembered it?{" "}
      <Link className="text-primary hover:underline" to="/login">
        Log in
      </Link>
    </p>
  );
}
