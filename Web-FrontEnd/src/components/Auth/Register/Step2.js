import React, { useEffect, useState } from "react";
import { Button, Col, Form } from "react-bootstrap";

import * as countryService from "../../../services/countryService";

const Step2 = ({
  user,
  onInputChange,
  genders,
  countries,
  prevStep,
  nextStep,
}) => {
  const [countriesCities, setCities] = useState([]);

  useEffect(() => {
    if (!!user.countryId) {
      countryService.getCitiesByCountryId(user.countryId).then((res) => {
        setCities((prevState) => {
          return [...prevState, res];
        });
      });
    }
  }, [user.countryId]);

  const currentCities = countriesCities.find(
    (cc) => cc.countryId === parseInt(user.countryId)
  );
  const areLoadedCities = !!currentCities;

  return (
    <div>
      <h4>Personal Information</h4>
      <Form.Group className="form-group mb-3 d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between">
        <Col className="d-flex" sm={3}>
          Gender:
        </Col>
        <Col className="d-flex">
          <div>
            {genders.map((g) => {
              return (
                <Form.Check
                  inline
                  label={g.name}
                  name="genderId"
                  type="radio"
                  key={`${g.id}-${g.name}`}
                  defaultChecked={g.id === +user.genderId || g.id === 1}
                  value={g.id}
                  onChange={onInputChange}
                />
              );
            })}
          </div>
        </Col>
      </Form.Group>
      <Form.Group
        className="form-group mb-3 d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between"
        controlId="countryId"
      >
        <Col className="d-flex" sm={3}>
          Country:
        </Col>
        <Col className="d-flex">
          <Form.Select
            name="countryId"
            value={user.countryId}
            onChange={onInputChange}
          >
            {countries.map((c, i) => {
              return (
                <option key={i} value={c.countryId}>
                  {c.countryName}
                </option>
              );
            })}
          </Form.Select>
        </Col>
      </Form.Group>
      {!!parseInt(user.countryId) && (
        <Form.Group
          className="form-group mb-3 d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between"
          controlId="cityId"
        >
          <Col className="d-flex" sm={3}>
            City:
          </Col>
          <Col className="d-flex">
            <Form.Select
              name="cityId"
              onChange={onInputChange}
              value={user.cityId}
            >
              {areLoadedCities &&
                currentCities.cities.map((c, i) => {
                  return (
                    <option key={i} value={c.cityId}>
                      {c.cityName}
                    </option>
                  );
                })}
            </Form.Select>
          </Col>
        </Form.Group>
      )}
      <Form.Group
        className="form-group mb-3 d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between"
        controlId="information"
      >
        <Col className="d-flex" sm={3}>
          Bio:
        </Col>
        <Col className="d-flex">
          <Form.Control
            as="textarea"
            name="bio"
            rows={3}
            value={user.bio}
            placeholder="Enter your bio"
            onChange={onInputChange}
            required
          />
        </Col>
      </Form.Group>
      <Form.Group className="d-flex justify-content-between align-items-center">
        <Button
          className="btn btn-outline-dark btn btn-light"
          onClick={prevStep}
        >
          Back
        </Button>
        <Button variant="dark" onClick={nextStep}>
          Next
        </Button>
      </Form.Group>
    </div>
  );
};

export default Step2;
