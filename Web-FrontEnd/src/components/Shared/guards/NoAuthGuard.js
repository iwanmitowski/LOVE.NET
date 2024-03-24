import { Navigate, Outlet } from "react-router-dom";
import { useIdentityContext } from "../../../hooks/useIdentityContext";

export default function NoAuthGuard({ children }) {
  const { isLogged } = useIdentityContext();

  if (isLogged) {
    return <Navigate to="/LOVE.NET" replace />;
  }

  return children ? children : <Outlet />;
}
