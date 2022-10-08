import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import { useState, useEffect, Fragment } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useIdentityContext } from "../../../hooks/useIdentityContext";

import * as identityService from "../../../services/identityService";

export default function Register() {
  const { userLogin } = useIdentityContext();
  const navigate = useNavigate();

  const [user, setUser] = useState({
    email: "",
    password: "",
    confirmPassword: "",
    userName: "",
    bio: "bio",
    birthdate: "",
    countryId: 0,
    cityId: 0,
  });

  const [error, setError] = useState("");

  useEffect(() => {
    console.log("Get countries and cities");
  }, []);

  const onInputChange = (e) => {
    setUser((prevState) => {
      let currentName = e.target.name;
      let currentValue = e.target.value;
      console.log(currentValue);
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
        navigate("/");
      })
      .catch((error) => {
        setError(error.message);
      });
  };

  return (
    <div className="register-form-wrapper">
      <Form onSubmit={onFormSubmit}>
        <Form.Group className="mb-3" controlId="email">
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
        <Form.Group className="mb-3" controlId="userName">
          <Form.Label>Username</Form.Label>
          <Form.Control
            type="text"
            name="userName"
            defaultValue={user.userName}
            placeholder="Enter username"
            onChange={onInputChange}
            required
          />
        </Form.Group>
        <Form.Group className="mb-3" controlId="password">
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
        <Form.Group className="mb-3" controlId="confirmPassword">
          <Form.Label>Confirm Password</Form.Label>
          <Form.Control
            type="password"
            name="confirmPassword"
            defaultValue={user.confirmPassword}
            placeholder="Confirm password"
            onChange={onInputChange}
            required
          />
        </Form.Group>
        <Form.Group className="mb-3" controlId="countryId">
          <Form.Label>Choose country</Form.Label>
          <Form.Select
            className="mb-3"
            name="countryId"
            onChange={onInputChange}
          >
            <option value="0">Chooce country here</option>
            <option value="1">first</option>
            <option value="2">second</option>
            <option value="3">third</option>
          </Form.Select>
        </Form.Group>
        {!!parseInt(user.countryId) && (
          <Form.Group className="mb-3" controlId="cityId">
            {" "}
            <Form.Label>Choose city</Form.Label>
            <Form.Select
              className="mb-3"
              name="cityId"
              onChange={onInputChange}
            >
              <option value="0">Choose city here</option>
              <option value="1">first</option>
              <option value="2">second</option>
              <option value="3">third</option>
            </Form.Select>
          </Form.Group>
        )}
        <Form.Group className="mb-3" controlId="information">
          <Form.Label>Bio</Form.Label>
          <Form.Control
            as="textarea"
            name="bio"
            rows={5}
            defaultValue={user.bio}
            placeholder="Enter your bio"
            onChange={onInputChange}
            required
          />
        </Form.Group>
        <Form.Group className="mb-3" controlId="profilePicture">
          <Form.Label>Upload your photo</Form.Label>
          <Form.Control type="file" />
        </Form.Group>
        {error && <span className="text-danger">{error}</span>}
        <Button variant="primary" type="submit">
          Register
        </Button>
        <Form.Group>
          <p>
            Already have account ?
            <Link className="nav-link" to="/login">
              Login
            </Link>
          </p>
        </Form.Group>
      </Form>
    </div>
  );
}
