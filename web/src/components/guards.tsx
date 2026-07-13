import { Navigate, Outlet } from "react-router";
import { isLogged, useAuthStore } from "@/stores/auth";

export function AuthGuard() {
  const logged = useAuthStore(isLogged);
  return logged ? <Outlet /> : <Navigate to="/welcome" replace />;
}

export function NoAuthGuard() {
  const logged = useAuthStore(isLogged);
  return logged ? <Navigate to="/" replace /> : <Outlet />;
}

export function AdminGuard() {
  const user = useAuthStore((s) => s.user);
  return user?.isAdmin ? <Outlet /> : <Navigate to="/" replace />;
}
