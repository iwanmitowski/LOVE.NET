import React from "react";
import { Button, Col, Form } from "react-bootstrap";
import { Link } from "react-router-dom";

const Step1 = ({ user, onInputChange, nextStep }) => {
  return (
    <div>
      <h4>Basic Information</h4>
      <Form.Group
        className="form-group mb-3 d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between"
        controlId="email"
      >
        <Col className="d-flex" sm={3}>
          Email:
        </Col>
        <Col className="d-flex">
          <Form.Control
            type="email"
            name="email"
            value={user.email}
            placeholder="Enter address"
            onChange={onInputChange}
            required
          />
        </Col>
      </Form.Group>
      <Form.Group
        className="form-group mb-3 d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between"
        controlId="userName"
      >
        <Col className="d-flex" sm={3}>
          Username:
        </Col>
        <Col className="d-flex">
          <Form.Control
            type="text"
            name="userName"
            value={user.userName}
            placeholder="Enter username"
            onChange={onInputChange}
            required
          />
        </Col>
      </Form.Group>
      <Form.Group
        className="form-group mb-3 d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between"
        controlId="birthdate"
      >
        <Col className="d-flex" sm={3}>
          Birthday:
        </Col>
        <Col>
          <span className="text-black">
            <Form.Control
              type="date"
              name="birthdate"
              value={new Date(user.birthdate).toLocaleDateString("en-CA", {
                year: "numeric",
                month: "2-digit",
                day: "2-digit",
              })}
              onChange={onInputChange}
            />
          </span>
        </Col>
      </Form.Group>
      <Form.Group className="d-flex justify-content-between align-items-center">
        <p className="d-flex gap-1 m-0">
          <span>Already have account?</span>
          <Link className="nav-link" to="/login">
            <u>Login</u>
          </Link>
        </p>
        <Button variant="dark" onClick={nextStep}>
          Next
        </Button>
      </Form.Group>
    </div>
  );
};

export default Step1;
