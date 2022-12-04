import Navbar from "react-bootstrap/NavBar";
import Nav from "react-bootstrap/Nav";
import Container from "react-bootstrap/Container";
import NavDropdown from "react-bootstrap/NavDropdown";
import { Link } from "react-router-dom";
import { Fragment } from "react";

import styles from "./Header.module.css";
import { useIdentityContext } from "../../../hooks/useIdentityContext";

export default function Header() {
  const { user, isLogged } = useIdentityContext();

  return (
    <header className={styles.header}>
      <Navbar collapseOnSelect expand="lg">
        <Container>
          <Navbar.Brand>
            <Link className="nav-link" to="/">
              LOVE.NET
            </Link>
          </Navbar.Brand>
          <Navbar.Toggle aria-controls="responsive-navbar-nav" />
          <Navbar.Collapse id="responsive-navbar-nav">
            <Nav className="me-auto">
              {isLogged && !user.isAdmin ? (
                <Link className="nav-link" to="/matches">
                  Matches
                </Link>
              ) : (
                <Link className="nav-link" to="/admin/dashboard">
                  Dashboard
                </Link>
              )}
            </Nav>
            <Nav>
              {!isLogged && (
                <Fragment>
                  <Link className="nav-link" to="/login">
                    Login
                  </Link>
                  <Link className="nav-link" to="/register">
                    Register
                  </Link>
                </Fragment>
              )}
              {isLogged && (
                <Fragment>
                  {user.isAdmin ? (
                    <div>
                      <Link className="nav-link"> {user.userName}</Link>
                    </div>
                  ) : (
                    <Link className="nav-link" to={`/user/${user.id}`}>
                      {user.userName}
                    </Link>
                  )}
                  <Nav>
                    <Link className="nav-link" to="/logout">
                      Logout
                    </Link>
                  </Nav>
                </Fragment>
              )}
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>
    </header>
  );
}
