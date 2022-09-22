import { Routes, Route } from "react-router-dom";
import "./App.css";
import Home from "./components/Home";
import Main from "./components/Shared/Main/Main";
import Footer from "./components/Shared/Footer/Footer";
import Header from "./components/Shared/Header/Header";

function App() {
  return (
    <div className="App">
      <Header />
      <Main >
        <Routes>
          <Route path="/" element={<Home />} />
        </Routes>
      </Main>
      <Footer />
    </div>
  );
}

export default App;
