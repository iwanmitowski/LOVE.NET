/* eslint-disable jsx-a11y/anchor-is-valid */
import React from "react";
import { Button, Col, Form } from "react-bootstrap";

const Step4 = ({
  user,
  onInputChange,
  setIsReading,
  prevStep,
}) => {
  return (
    <div>
      <h4>Finalize</h4>
      <Form.Group
        className="form-group mb-3 d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between"
        controlId="email"
      >
        <Form.Control
          type="password"
          name="password"
          value={user.password || ""}
          placeholder="Enter password"
          onChange={onInputChange}
          required
        />
      </Form.Group>
      <Form.Group
        className="form-group mb-3 d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between"
        controlId="userName"
      >
        <Col className="d-flex">
          <Form.Control
            type="password"
            name="confirmPassword"
            value={user.confirmPassword || ""}
            placeholder="Confirm password"
            onChange={onInputChange}
            required
          />
        </Col>
      </Form.Group>
      <Form.Group
        className="form-group mb-3 d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center"
        controlId="birthdate"
      >
        <Form.Check
          name="isAgreed"
          checked={user.isAgreed}
          onChange={onInputChange}
          required
          className="d-inline me-2"
        />
        <a
          href="#"
          className="d-inline"
          onClick={(e) => {
            e.preventDefault();
            setIsReading(true);
          }}
        >
          Read our terms and conditions...
        </a>
      </Form.Group>
      <Form.Group className="d-flex justify-content-between align-items-center">
        <Button
          className="btn btn-outline-dark btn btn-light"
          onClick={prevStep}
        >
          Back
        </Button>
        <Button
          variant="dark"
          disabled={user.isAgreed ? null : true}
          type="submit"
        >
          Register
        </Button>
      </Form.Group>
    </div>
  );
};

export default Step4;
