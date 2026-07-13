import { Bell, Flame, Heart, MessageCircle, Quote } from "lucide-react";
import { motion } from "motion/react";
import { Link } from "react-router";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";

const features = [
  {
    icon: Flame,
    title: "Swipe & match",
    description:
      "Discover people near you and around the world. Like who catches your eye — when it's mutual, it's a match.",
  },
  {
    icon: MessageCircle,
    title: "Chat in realtime",
    description:
      "Every match opens a private room. Messages, photos, and typing indicators arrive instantly.",
  },
  {
    icon: Bell,
    title: "Stay notified",
    description:
      "New match? New message? You'll know the moment it happens, on any device.",
  },
] as const;

// Condensed from the classic LOVE.NET dating advice.
const advices = [
  "Be yourself — genuine beats impressive when building a real connection.",
  "Communicate openly and honestly; trust is what turns dates into relationships.",
  "Stay safe: meet in public and let a friend know where you're going.",
  "Be patient — great relationships take time, so enjoy getting to know each other.",
] as const;

const fadeUp = {
  initial: { opacity: 0, y: 16 },
  animate: { opacity: 1, y: 0 },
} as const;

export default function LandingPage() {
  return (
    <div className="flex min-h-dvh flex-col">
      <main className="mx-auto flex w-full max-w-5xl flex-1 flex-col items-center px-4">
        {/* Hero */}
        <motion.section
          className="flex flex-col items-center pt-20 pb-16 text-center md:pt-28"
          initial={fadeUp.initial}
          animate={fadeUp.animate}
          transition={{ duration: 0.5, ease: "easeOut" }}
        >
          <span className="gradient-brand grid size-16 place-items-center rounded-3xl shadow-lg">
            <Heart className="size-8 text-white" fill="currentColor" />
          </span>
          <h1 className="text-gradient-brand mt-6 text-5xl font-extrabold tracking-tight md:text-6xl">
            LOVE.NET
          </h1>
          <p className="text-muted-foreground mt-4 max-w-md text-lg text-balance">
            Match with people around the world and chat in real time.
          </p>
          <div className="mt-8 flex flex-col gap-3 sm:flex-row">
            <Button asChild size="lg" className="gradient-brand border-0 text-white hover:opacity-90">
              <Link to="/register">Create account</Link>
            </Button>
            <Button asChild size="lg" variant="outline">
              <Link to="/login">Log in</Link>
            </Button>
          </div>
        </motion.section>

        {/* Features */}
        <section className="grid w-full gap-4 pb-16 md:grid-cols-3">
          {features.map((feature, index) => (
            <motion.div
              key={feature.title}
              initial={fadeUp.initial}
              animate={fadeUp.animate}
              transition={{ duration: 0.5, ease: "easeOut", delay: 0.15 + index * 0.1 }}
            >
              <Card className="h-full">
                <CardContent className="flex h-full flex-col gap-3 pt-6">
                  <span className="bg-primary/10 text-primary grid size-11 place-items-center rounded-xl">
                    <feature.icon className="size-5" />
                  </span>
                  <h2 className="text-lg font-semibold">{feature.title}</h2>
                  <p className="text-muted-foreground text-sm">{feature.description}</p>
                </CardContent>
              </Card>
            </motion.div>
          ))}
        </section>

        {/* Dating advice strip */}
        <motion.section
          className="w-full pb-16"
          initial={fadeUp.initial}
          animate={fadeUp.animate}
          transition={{ duration: 0.5, ease: "easeOut", delay: 0.45 }}
        >
          <h2 className="text-muted-foreground mb-4 text-center text-sm font-medium tracking-wide uppercase">
            Dating advice
          </h2>
          <div className="grid gap-3 sm:grid-cols-2">
            {advices.map((advice) => (
              <div
                key={advice}
                className="bg-card flex items-start gap-3 rounded-xl border p-4"
              >
                <Quote className="text-primary mt-0.5 size-4 shrink-0" />
                <p className="text-muted-foreground text-sm">{advice}</p>
              </div>
            ))}
          </div>
        </motion.section>
      </main>

      <footer className="text-muted-foreground border-t py-6 text-center text-sm">
        © {new Date().getFullYear()} LOVE.NET
      </footer>
    </div>
  );
}
