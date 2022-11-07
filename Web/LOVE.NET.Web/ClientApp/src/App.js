import { Routes, Route } from "react-router-dom";
import { IdentityProvider } from "./contexts/IdentityContext";
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

import "./App.css";
import UserDetails from "./components/User/UserDetails";

function App() {
  return (
    <IdentityProvider>
      <div className="App">
        <Header />
        <Main>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/login" element={<NoAuthGuard><Login /></NoAuthGuard>} />
            <Route path="/logout" element={<AuthGuard><Logout /></AuthGuard>} />
            <Route path="/register" element={<NoAuthGuard><Register /></NoAuthGuard>} />
            <Route path="/verify" element={<NoAuthGuard><Verify /></NoAuthGuard>} />
            <Route path="/user/:id" element={<AuthGuard><UserDetails /></AuthGuard>} />
          </Routes>
        </Main>
        <Footer />
      </div>
    </IdentityProvider>
  );
}

export default App;
