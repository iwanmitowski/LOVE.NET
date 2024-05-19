import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useIdentityContext } from "../../../hooks/useIdentityContext";

import * as identityService from "../../../services/identityService";
import styles from "../../Shared/Forms.module.css";

export default function Login() {
  const { userLogin } = useIdentityContext();
  const navigate = useNavigate();

  const [user, setUser] = useState({
    email: "",
    password: "",
  });

  const [error, setError] = useState("");

  const onInputChange = (e) => {
    setUser((prevState) => {
      let currentName = e.target.name;
      let currentValue = e.target.value;

      return {
        ...prevState,
        [currentName]: currentValue,
      };
    });

    setError("");
  };

  const onFormSubmit = (e) => {
    e.preventDefault();

    identityService
      .login(user)
      .then((res) => {
        userLogin(res);
        navigate("/");
      })
      .catch((error) => {
        setError(error.message);
      });
  };

  const formWrapperStyles = `${styles["form-wrapper"]} d-flex flex-column justify-content-center align-items-center`;

  return (
    <div className={formWrapperStyles}>
      <div className="bg-light rounded shadow p-3">
        <h1 className="pb-2">Login</h1>
        <div className={styles["input-fields-length"]}>
          <Form onSubmit={onFormSubmit}>
            <Form.Group className="form-group mb-3" controlId="email">
              <Form.Control
                type="email"
                name="email"
                defaultValue={user.email}
                placeholder="Email"
                onChange={onInputChange}
                required
              />
            </Form.Group>
            <Form.Group className="form-group mb-3" controlId="password">
              <Form.Control
                type="password"
                name="password"
                defaultValue={user.password}
                placeholder="Password"
                onChange={onInputChange}
                required
              />
            </Form.Group>
            {error && (
              <div className="text-danger mb-3">
                {error.split("\n").map((message, key) => {
                  return <div key={key}>{message}</div>;
                })}
              </div>
            )}
            <Form.Group>
              <p>
                <Link className="nav-link" to="/resetPassword">
                  Forgot your password ?
                </Link>
              </p>
            </Form.Group>
            <Button variant="dark" type="submit">
              Login
            </Button>
            <Form.Group>
              <p className="pt-2">
                <span>You don't have account?</span>
                <Link className="nav-link" to="/register">
                  <u>Register</u>
                </Link>
              </p>
            </Form.Group>
          </Form>
        </div>
      </div>
    </div>
  );
}
