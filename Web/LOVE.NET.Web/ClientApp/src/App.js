/* eslint-disable react-hooks/exhaustive-deps */
import { useEffect } from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import Home from "./components/Home";
import Main from "./components/Shared/Main/Main";
import Footer from "./components/Shared/Footer/Footer";
import Header from "./components/Shared/Header/Header";
import NoAuthGuard from "./components/Shared/guards/NoAuthGuard";
import AuthGuard from "./components/Shared/guards/AuthGuard";
import Login from "./components/Auth/Login/Login";
import Logout from "./components/Auth/Logout/Logout";
import Register from "./components/Auth/Register/Register";
import Verify from "./components/Auth/Verify/Verify";
import Forbidden from "./components/Shared/Error/Forbidden/Forbidden";
import NotFound from "./components/Shared/Error/NotFound/NotFound";
import UserDetails from "./components/User/UserDetails";
import Matches from "./components/Matches";

import { useIdentityContext } from "./hooks/useIdentityContext";
import * as identityService from "./services/identityService";
import "./App.css";
import Dashboard from "./components/Admin/Dashboard/Dashboard";
import AdminGuard from "./components/Shared/guards/AdminGuard";

function App() {
  const { isLogged, userLogout } = useIdentityContext();

  // Logout if session is expired
  useEffect(() => {
    if (isLogged) {
      identityService.refreshToken().catch(() => userLogout());
    }
  }, []);

  return (
    <div className="App">
      <Header />
      <Main>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route
            path="/admin/dashboard"
            element={
              <AuthGuard>
                <AdminGuard>
                  <Dashboard />
                </AdminGuard>
              </AuthGuard>
            }
          />
          <Route
            path="/matches"
            element={
              <AuthGuard>
                <Matches />
              </AuthGuard>
            }
          />
          <Route
            path="/login"
            element={
              <NoAuthGuard>
                <Login />
              </NoAuthGuard>
            }
          />
          <Route
            path="/logout"
            element={
              <AuthGuard>
                <Logout />
              </AuthGuard>
            }
          />
          <Route
            path="/register"
            element={
              <NoAuthGuard>
                <Register />
              </NoAuthGuard>
            }
          />
          <Route
            path="/verify"
            element={
              <NoAuthGuard>
                <Verify />
              </NoAuthGuard>
            }
          />
          <Route
            path="/user/:id"
            element={
              <AuthGuard>
                <UserDetails />
              </AuthGuard>
            }
          />
          <Route path="/forbidden" element={<Forbidden />} />
          <Route path="/notfound" element={<NotFound />} />
          <Route path="*" element={<Navigate to="/notfound" replace />} />
        </Routes>
      </Main>
      <Footer />
    </div>
  );
}

export default App;
