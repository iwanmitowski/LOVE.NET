import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { Heart } from "lucide-react";
import { useForm } from "react-hook-form";
import { Link, useNavigate } from "react-router";
import { z } from "zod";
import { login } from "@/api/identity";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { validation } from "@/lib/constants";
import { errorMessages } from "@/lib/errors";
import { useAuthStore } from "@/stores/auth";

const schema = z.object({
  email: z.email("Invalid email"),
  password: z.string().min(validation.PASSWORD_MIN_LENGTH, "Password is too short"),
});

type FormValues = z.infer<typeof schema>;

export default function LoginPage() {
  const storeLogin = useAuthStore((s) => s.login);
  const navigate = useNavigate();

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { email: "", password: "" },
  });

  const mutation = useMutation({
    mutationFn: (values: FormValues) => login(values.email, values.password),
    onSuccess: (data) => {
      storeLogin(data);
      void navigate(data.isAdmin ? "/admin" : "/", { replace: true });
    },
  });

  return (
    <div className="grid min-h-dvh place-items-center p-4">
      <Card className="w-full max-w-sm">
        <CardHeader className="items-center text-center">
          <span className="gradient-brand mx-auto grid size-12 place-items-center rounded-2xl">
            <Heart className="size-6 text-white" fill="currentColor" />
          </span>
          <h1 className="text-gradient-brand text-2xl font-extrabold">Welcome back</h1>
          <p className="text-muted-foreground text-sm">Log in to keep the sparks flying</p>
        </CardHeader>
        <CardContent>
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
              <FormField
                control={form.control}
                name="password"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Password</FormLabel>
                    <FormControl>
                      <Input type="password" autoComplete="current-password" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {mutation.isError && (
                <div className="text-destructive space-y-0.5 text-sm">
                  {errorMessages(mutation.error).map((line) => (
                    <p key={line}>{line}</p>
                  ))}
                </div>
              )}

              <Button type="submit" className="w-full" disabled={mutation.isPending}>
                {mutation.isPending ? "Logging in…" : "Log in"}
              </Button>
            </form>
          </Form>

          <div className="text-muted-foreground mt-4 space-y-1 text-center text-sm">
            <p>
              Forgot your password?{" "}
              <Link className="text-primary hover:underline" to="/resetPassword">
                Reset it
              </Link>
            </p>
            <p>
              New here?{" "}
              <Link className="text-primary hover:underline" to="/register">
                Create an account
              </Link>
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
