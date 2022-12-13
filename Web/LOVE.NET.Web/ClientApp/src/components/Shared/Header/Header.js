import { Link } from "react-router-dom";
import { Fragment } from "react";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faUsers,
  faTachometer,
  faSignIn,
  faSignOut,
  faUser,
  faFilePen,
  faWind,
} from "@fortawesome/free-solid-svg-icons";

import { useIdentityContext } from "../../../hooks/useIdentityContext";
import { ButtonGroup, Dropdown, DropdownButton } from "react-bootstrap";

export default function Header() {
  const { user, isLogged } = useIdentityContext();

  return (
    <div
      className="d-flex flex-column flex-shrink-0 p-3 bg-light fixed-top"
      style={{ width: "280px", minHeight: "100vh", textAlign: "left" }}
    >
      <Link className="nav-link" to="/">
        LOVE.NET
      </Link>
      <hr />
      <ul className="nav nav-pills flex-column mb-auto">
        {isLogged && !user.isAdmin && (
          <Fragment>
            <li className="nav-item">
              <Link className="nav-link" to="/">
                <FontAwesomeIcon icon={faWind} /> Swipe
              </Link>
            </li>
            <li className="nav-item">
              <Link className="nav-link" to="/matches">
                <FontAwesomeIcon icon={faUsers} /> Matches
              </Link>
            </li>
          </Fragment>
        )}
        {isLogged && user.isAdmin && (
          <Link className="nav-link" to="/admin/dashboard">
            <FontAwesomeIcon icon={faTachometer} />
            {" Dashboard"}
          </Link>
        )}
        {!isLogged && (
          <Fragment>
            <li className="nav-item">
              <Link className="nav-link" to="/login">
                <FontAwesomeIcon icon={faSignIn} /> Login
              </Link>
            </li>
            <li className="nav-item">
              <Link className="nav-link" to="/register">
                <FontAwesomeIcon icon={faFilePen} /> Register
              </Link>
            </li>
          </Fragment>
        )}
      </ul>
      <hr />
      {isLogged && (
        <DropdownButton
          as={ButtonGroup}
          key="danger"
          drop="up"
          id={`actions-dropdown`}
          variant="light"
          title={
            isLogged && (
              <div className="d-inline-flex text-wrap">
                <img
                  src={user.profilePicture}
                  alt=""
                  width="25"
                  height="25"
                  className="rounded-circle me-2"
                ></img>
                <strong>Actions</strong>
              </div>
            )
          }
        >
          {isLogged && !user.isAdmin && (
            <Fragment>
              <Dropdown.Item eventKey="1">
                <Link className="nav-link" to={`/user/${user.id}`}>
                  <FontAwesomeIcon icon={faUser} /> Profile
                </Link>
              </Dropdown.Item>
              <Dropdown.Divider />
            </Fragment>
          )}
          <Dropdown.Item eventKey="2">
            <Link className="nav-link" to="/logout">
              <FontAwesomeIcon icon={faSignOut} /> Logout
            </Link>
          </Dropdown.Item>
        </DropdownButton>
      )}
    </div>
  );
}
