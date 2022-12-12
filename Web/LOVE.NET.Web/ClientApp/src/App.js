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
import UsersContainer from "./components/Admin/Users/UsersContainer";

function App() {
  const { isLogged, userLogout, user } = useIdentityContext();

  // Logout if session is expired
  useEffect(() => {
    if (isLogged) {
      identityService.refreshToken().catch(() => userLogout());
    }
  }, []);

  const homeComponent = isLogged && user.isAdmin ? <Dashboard /> : <Home />;

  return (
    <div className="App">
      <Main>
        <Header />
        <div style={{ minWidth: "280px" }}></div>
        <div className="col-md-10">
          <div>
            <div className="p-3" style={{minHeight: "100vh"}}>
              <Routes style={{ overflowX: "hidden", minHeight: "100vh" }}>
                <Route path="/" element={homeComponent} />
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
                  path="/admin/users"
                  element={
                    <AuthGuard>
                      <AdminGuard>
                        <UsersContainer />
                      </AdminGuard>
                    </AuthGuard>
                  }
                />
                <Route
                  path="/admin/users/banned"
                  element={
                    <AuthGuard>
                      <AdminGuard>
                        <UsersContainer showBanned />
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
            </div>
          </div>

          <Footer />
        </div>
      </Main>
    </div>
  );
}

export default App;
