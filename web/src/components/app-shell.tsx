import { Flame, Heart, MessageCircle, Moon, ShieldCheck, Sun, User } from "lucide-react";
import { NavLink, Outlet, useNavigate } from "react-router";
import { logout as apiLogout } from "@/api/identity";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useTheme } from "@/components/theme-provider";
import { useConversations } from "@/hooks/use-conversations";
import { cn } from "@/lib/utils";
import { useAuthStore } from "@/stores/auth";

interface NavItem {
  to: string;
  label: string;
  icon: typeof Flame;
  badge?: number;
  adminOnly?: boolean;
}

export default function AppShell() {
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const { theme, toggle } = useTheme();
  const navigate = useNavigate();
  const { totalUnread } = useConversations();

  const items: NavItem[] = user?.isAdmin
    ? [{ to: "/admin", label: "Dashboard", icon: ShieldCheck }]
    : [
        { to: "/", label: "Discover", icon: Flame },
        { to: "/messages", label: "Messages", icon: MessageCircle, badge: totalUnread },
        { to: "/profile", label: "Profile", icon: User },
      ];

  async function onLogout() {
    try {
      await apiLogout();
    } finally {
      logout();
      void navigate("/welcome", { replace: true });
    }
  }

  return (
    <div className="flex h-dvh flex-col md:flex-row">
      {/* Desktop rail */}
      <aside className="bg-card hidden w-60 shrink-0 flex-col border-r md:flex">
        <NavLink to="/" className="flex items-center gap-2 px-5 py-6">
          <span className="gradient-brand grid size-9 place-items-center rounded-xl">
            <Heart className="size-5 text-white" fill="currentColor" />
          </span>
          <span className="text-gradient-brand text-lg font-extrabold tracking-tight">
            LOVE.NET
          </span>
        </NavLink>

        <nav className="flex flex-1 flex-col gap-1 px-3">
          {items.map((item) => (
            <RailLink key={item.to} item={item} />
          ))}
        </nav>

        <div className="flex items-center justify-between border-t p-3">
          <DropdownMenu>
            <DropdownMenuTrigger className="flex min-w-0 items-center gap-2 rounded-lg p-1.5 hover:bg-accent">
              <Avatar className="size-8">
                <AvatarImage src={user?.profilePicture ?? undefined} />
                <AvatarFallback>{user?.userName?.[0]?.toUpperCase()}</AvatarFallback>
              </Avatar>
              <span className="truncate text-sm font-medium">{user?.userName}</span>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="start" side="top">
              {!user?.isAdmin && (
                <DropdownMenuItem onClick={() => void navigate("/profile")}>
                  Profile
                </DropdownMenuItem>
              )}
              <DropdownMenuItem onClick={() => void onLogout()}>Log out</DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
          <Button variant="ghost" size="icon" onClick={toggle} aria-label="Toggle theme">
            {theme === "dark" ? <Sun className="size-4" /> : <Moon className="size-4" />}
          </Button>
        </div>
      </aside>

      {/* Content */}
      <main className="min-h-0 flex-1 overflow-y-auto">
        <Outlet />
      </main>

      {/* Mobile bottom tabs */}
      <nav className="bg-card safe-bottom flex shrink-0 items-stretch justify-around border-t md:hidden">
        {items.map((item) => (
          <TabLink key={item.to} item={item} />
        ))}
        <button
          className="text-muted-foreground flex flex-1 flex-col items-center gap-0.5 py-2 text-[11px]"
          onClick={() => void onLogout()}
        >
          <User className="size-5" />
          Log out
        </button>
      </nav>
    </div>
  );
}

function RailLink({ item }: { item: NavItem }) {
  return (
    <NavLink
      to={item.to}
      end={item.to === "/"}
      className={({ isActive }) =>
        cn(
          "flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors",
          isActive ? "bg-primary/10 text-primary" : "text-muted-foreground hover:bg-accent hover:text-foreground",
        )
      }
    >
      <item.icon className="size-5" />
      <span className="flex-1">{item.label}</span>
      {!!item.badge && (
        <Badge className="gradient-brand border-0 text-white">{item.badge}</Badge>
      )}
    </NavLink>
  );
}

function TabLink({ item }: { item: NavItem }) {
  return (
    <NavLink
      to={item.to}
      end={item.to === "/"}
      className={({ isActive }) =>
        cn(
          "relative flex flex-1 flex-col items-center gap-0.5 py-2 text-[11px]",
          isActive ? "text-primary" : "text-muted-foreground",
        )
      }
    >
      <item.icon className="size-5" />
      {item.label}
      {!!item.badge && (
        <span className="gradient-brand absolute top-1 right-1/2 grid size-4 -translate-x-[-14px] place-items-center rounded-full text-[9px] font-bold text-white">
          {item.badge > 9 ? "9+" : item.badge}
        </span>
      )}
    </NavLink>
  );
}
