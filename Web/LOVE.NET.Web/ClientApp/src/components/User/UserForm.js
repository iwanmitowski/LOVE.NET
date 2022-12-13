import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import { useState, useEffect } from "react";
import { useIdentityContext } from "../../hooks/useIdentityContext";

import * as countryService from "../../services/countryService";

import styles from "../Shared/Forms.module.css";

export default function UserForm(props) {
  const user = props.user;
  const genders = props.genders;
  const countriesProp = props.countries;
  const onFormSubmit = props.onFormSubmit;
  const onInputChange = props.onInputChange;
  const [error] = props.errorState;

  const { isLogged } = useIdentityContext();
  const [countries, setCountries] = useState([]);
  const [countriesCities, setCities] = useState([]);

  useEffect(() => {
    setCountries(countriesProp);
  }, [countriesProp]);

  useEffect(() => {
    if (!!user.countryId) {
      countryService.getCitiesByCountryId(user.countryId).then((res) => {
        setCities((prevState) => {
          return [...prevState, res];
        });
      });
    }
  }, [user.countryId]);

  const formWrapperStyles = `${styles["form-wrapper"]} d-flex justify-content-center align-items-center`;
  const currentCities = countriesCities.find(
    (cc) => cc.countryId === parseInt(user.countryId)
  );

  const areLoadedCities = !!currentCities;

  return (
    <div className={formWrapperStyles}>
      <div className={styles["input-fields-length"]}>
        <Form onSubmit={onFormSubmit}>
          {!isLogged && (
            <Form.Group className="form-group mb-3" controlId="email">
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
          )}

          <Form.Group className="form-group mb-3" controlId="userName">
            <Form.Label>Username</Form.Label>
            <Form.Control
              type="text"
              name="userName"
              value={user.userName}
              placeholder="Enter username"
              onChange={onInputChange}
              required
            />
          </Form.Group>
          <Form.Group className="form-group mb-3" controlId="birthdate">
            <Form.Label>Enter your birthdate</Form.Label>
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
          </Form.Group>
          <Form.Group className="form-group mb-3">
            <Form.Label>Choose gender</Form.Label>
            <div>
              {genders.map((g) => {
                return (
                  <Form.Check
                    inline
                    label={g.name}
                    name="genderId"
                    type="radio"
                    key={`${g.id}-${g.name}`}
                    defaultChecked={
                      isLogged ? g.id === user.genderId : g.id === 1
                    }
                    value={g.id}
                    onChange={onInputChange}
                  />
                );
              })}
            </div>
          </Form.Group>
          <Form.Group className="form-group mb-3" controlId="countryId">
            <Form.Label>Choose country</Form.Label>
            <Form.Select
              className="mb-3"
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
          </Form.Group>
          {!!parseInt(user.countryId) && (
            <Form.Group className="form-group mb-3" controlId="cityId">
              <Form.Label>Choose city</Form.Label>
              <Form.Select
                className="mb-3"
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
            </Form.Group>
          )}
          <Form.Group className="form-group mb-3" controlId="information">
            <Form.Label>Bio</Form.Label>
            <Form.Control
              as="textarea"
              name="bio"
              rows={5}
              value={user.bio}
              placeholder="Enter your bio"
              onChange={onInputChange}
              required
            />
          </Form.Group>
          {!isLogged && (
            <Form.Group className="form-group mb-3" controlId="profilePicture">
              <Form.Label>Upload your profile picture</Form.Label>
              <Form.Control
                type="file"
                name="image"
                onChange={onInputChange}
                accept=".jpg,.jpeg,.png"
              />
            </Form.Group>
          )}
          <Form.Group className="form-group mb-3" controlId="images">
            <Form.Label>Upload your new images</Form.Label>
            <Form.Control
              type="file"
              name="newImages"
              multiple
              value={user?.newImages?.length ? null : '' }
              onChange={onInputChange}
              accept=".jpg,.jpeg,.png"
            />
          </Form.Group>
          <Form.Group className="form-group mb-3" controlId="password">
            <Form.Label>Password</Form.Label>
            <Form.Control
              type="password"
              name="password"
              value={user.password || ""}
              placeholder="Enter password"
              onChange={onInputChange}
              required
            />
          </Form.Group>
          <Form.Group className="form-group mb-3" controlId="confirmPassword">
            <Form.Label>Confirm Password</Form.Label>
            <Form.Control
              type="password"
              name="confirmPassword"
              value={user.confirmPassword || ""}
              placeholder="Confirm password"
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
          <Button variant="dark" type="submit">
            {isLogged ? "Edit" : "Register"}
          </Button>
        </Form>
      </div>
    </div>
  );
}
