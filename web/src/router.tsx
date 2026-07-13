import { createBrowserRouter, Navigate } from "react-router";
import AppShell from "@/components/app-shell";
import { AdminGuard, AuthGuard, NoAuthGuard } from "@/components/guards";
import DashboardPage from "@/features/admin/dashboard-page";
import UsersPage from "@/features/admin/users-page";
import LoginPage from "@/features/auth/login-page";
import RegisterPage from "@/features/auth/register-page";
import ResetPage from "@/features/auth/reset-page";
import VerifyPage from "@/features/auth/verify-page";
import DiscoverPage from "@/features/discover/discover-page";
import LandingPage from "@/features/landing/landing-page";
import MessagesPage from "@/features/messages/messages-page";
import ProfilePage from "@/features/profile/profile-page";

// Exported instance (not just JSX) so non-component code — e.g. toast
// actions — can navigate via router.navigate().
export const router = createBrowserRouter([
  {
    element: <NoAuthGuard />,
    children: [
      { path: "/welcome", element: <LandingPage /> },
      { path: "/login", element: <LoginPage /> },
      { path: "/register", element: <RegisterPage /> },
      { path: "/verify", element: <VerifyPage /> },
    ],
  },
  // Reachable both logged in and out (old-app behavior).
  { path: "/resetPassword", element: <ResetPage /> },
  {
    element: <AuthGuard />,
    children: [
      {
        element: <AppShell />,
        children: [
          { path: "/", element: <DiscoverPage /> },
          { path: "/messages", element: <MessagesPage /> },
          { path: "/messages/:roomId", element: <MessagesPage /> },
          { path: "/profile", element: <ProfilePage /> },
          {
            element: <AdminGuard />,
            children: [
              { path: "/admin", element: <DashboardPage /> },
              { path: "/admin/users", element: <UsersPage /> },
            ],
          },
        ],
      },
    ],
  },
  { path: "*", element: <Navigate to="/" replace /> },
]);
