import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useIdentityContext } from "../../../hooks/useIdentityContext";

import * as identityService from "../../../services/identityService";
import styles from "../Auth.module.css";

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

  const formWrapperStyles = `${styles["form-wrapper"]} d-flex justify-content-center align-items-center`;

  return (
    <div className={formWrapperStyles}>
      <div className={styles["input-fields-length"]}>
        <Form onSubmit={onFormSubmit}>
          <Form.Group className="form-group mb-3" controlId="email">
            <Form.Label>Email</Form.Label>
            <Form.Control
              type="email"
              name="email"
              defaultValue={user.email}
              placeholder="Enter address"
              onChange={onInputChange}
              required
            />
          </Form.Group>
          <Form.Group className="form-group mb-3" controlId="password">
            <Form.Label>Password</Form.Label>
            <Form.Control
              type="password"
              name="password"
              defaultValue={user.password}
              placeholder="Enter password"
              onChange={onInputChange}
              required
            />
          </Form.Group>
          {error && <div className="text-danger mb-3"><span>{error}</span></div>}
          <Button variant="primary" type="submit">
            Login
          </Button>
          <Form.Group>
            <p>
              You don't have account ?
              <Link className="nav-link" to="/register">
                Register
              </Link>
            </p>
          </Form.Group>
        </Form>
      </div>
    </div>
  );
}
