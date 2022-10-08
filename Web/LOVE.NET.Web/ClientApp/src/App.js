import { Routes, Route } from "react-router-dom";
import { IdentityProvider } from "./contexts/IdentityContext";
import Home from "./components/Home";
import Main from "./components/Shared/Main/Main";
import Footer from "./components/Shared/Footer/Footer";
import Header from "./components/Shared/Header/Header";
import Login from "./components/Auth/Login/Login";
import Logout from "./components/Auth/Logout/Logout";
import Register from "./components/Auth/Register/Register";

import "./App.css";

function App() {
  return (
    <IdentityProvider>
      <div className="App">
        <Header />
        <Main>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/login" element={<Login />} />
            <Route path="/logout" element={<Logout />} />
            <Route path="/register" element={<Register />} />
          </Routes>
        </Main>
        <Footer />
      </div>
    </IdentityProvider>
  );
}

export default App;
