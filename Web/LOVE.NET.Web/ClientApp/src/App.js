import { Routes, Route } from "react-router-dom";
import "./App.css";
import Home from "./components/Home";
import Main from "./components/Shared/Main/Main";
import Footer from "./components/Shared/Footer/Footer";
import Header from "./components/Shared/Header/Header";
import Login from "./components/Auth/Login/Login";
import { IdentityProvider } from "./contexts/IdentityContext";
import Logout from "./components/Auth/Logout/Logout";

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
          </Routes>
        </Main>
        <Footer />
      </div>
    </IdentityProvider>
  );
}

export default App;
