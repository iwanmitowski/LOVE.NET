import Navbar from "react-bootstrap/NavBar";
import Nav from "react-bootstrap/Nav";
import Container from "react-bootstrap/Container";
import NavDropdown from "react-bootstrap/NavDropdown";
import { Link } from "react-router-dom";
import { Fragment } from "react";

import styles from "./Header.module.css";

export default function Header() {
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
              <Link className="nav-link" to="/likes">
                Likes
              </Link>
              <Link className="nav-link" to="/matches">
                Matches
              </Link>
              <NavDropdown
                title="Dating information"
                id="collasible-nav-dropdown"
              >
                <NavDropdown.Item href="#action/3.1">Info 1</NavDropdown.Item>
                <NavDropdown.Item href="#action/3.2">Info 2</NavDropdown.Item>
                <NavDropdown.Item href="#action/3.3">Info 3</NavDropdown.Item>
                <NavDropdown.Divider />
                <NavDropdown.Item href="#action/3.4">Info 4</NavDropdown.Item>
              </NavDropdown>
            </Nav>
            <Nav>
              <Fragment>
                <Link className="nav-link" to="/login">
                  Login
                </Link>
                <Link className="nav-link" to="/register">
                  Register
                </Link>
                <Link className="nav-link" to="/user">
                  User name
                </Link>
                <Nav>
                  <Link className="nav-link" to="/">
                    Logout
                  </Link>
                </Nav>
              </Fragment>
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>
    </header>
  );
}
