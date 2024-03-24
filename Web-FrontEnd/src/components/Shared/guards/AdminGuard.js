import { Navigate, Outlet } from "react-router-dom";
import { useIdentityContext } from "../../../hooks/useIdentityContext";

export default function AdminGuard({ children }) {
  const { user } = useIdentityContext();

  if (!user.isAdmin) {
    return <Navigate to="/forbidden" replace />;
  }

  return children ? children : <Outlet />;
}
