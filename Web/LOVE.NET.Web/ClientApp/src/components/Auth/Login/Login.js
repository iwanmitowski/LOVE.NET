import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useIdentityContext } from "../../../hooks/useIdentityContext";
import * as identityService from "../../../services/identityService";


export default function Login() {
	const { userLogin	} = useIdentityContext();
  const navigate = useNavigate();

  const [user, setUser] = useState({
    name: "",
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

		identityService.login(user)
      .then((res) => {
				userLogin(res);
        navigate("/");
      })
      .catch((error) => {
        setError(error.message);
      });
  };

  return (
    <div className="login-form-wrapper">
      <Form onSubmit={onFormSubmit}>
        <Form.Group className="mb-3" controlId="email">
          <Form.Label>Email</Form.Label>
          <Form.Control
            type="email"
            name="email"
            value={user.email}
            placeholder="Enter address"
            onChange={onInputChange}
            required
          />
        </Form.Group>
        <Form.Group className="mb-3" controlId="password">
          <Form.Label>Password</Form.Label>
          <Form.Control
            type="password"
            name="password"
            value={user.password}
            placeholder="Enter password"
            onChange={onInputChange}
            required
          />
        </Form.Group>
				{error && <span className="text-danger">{error}</span>}
        <Form.Group>
          <p>
            You don't have account ?
            <Link className="nav-link" to="/register">
              Register
            </Link>
          </p>
        </Form.Group>
        <Button variant="primary" type="submit">
          Login
        </Button>
      </Form>
    </div>
  );
}
