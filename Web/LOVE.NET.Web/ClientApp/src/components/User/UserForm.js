import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import { useState, useEffect, useCallback } from "react";
import { useIdentityContext } from "../../hooks/useIdentityContext";

import * as countryService from "../../services/countryService";

import styles from "../Shared/Forms.module.css";

export default function UserForm(props) {
  const user = props.user;
  const genders = props.genders;
  const townState = props.townState;
  const onFormSubmit = props.onFormSubmit;
  const onInputChange = props.onInputChange;
  const [error, setError] = props.errorState;

  const { isLogged } = useIdentityContext();
  const [countries, setCountries] = useState(townState.country);
  const [countriesCities, setCities] = useState([
    {
      cityId: 0,
      cityName: "Choose city here",
    },
  ]);

  const loadCities = useCallback(() => {
    const countryId = user?.country.countryId || user.countryId;

    const isCountryFetched = countriesCities.some(
      (c) => c.countryId === parseInt(countryId)
    );
    
    if (countryId !== 0 && !isCountryFetched) {
      countryService.getCitiesByCountryId(countryId).then((res) => {
        setCities((prevState) => {
          return [...prevState, res];
        });
      });
    }
  });

  useEffect(() => {
    if (!countries.length) {
      setCountries(townState.country);
    }

    if (!countriesCities.length) {
      setCities(townState.city);
    }

    if (countries.length === 1) {
      countryService
        .getAll()
        .then((res) => {
          setCountries((prevState) => {
            return [...prevState, ...res];
          });
        })
        .catch((error) => {
          setError(error.message);
        })
        .finally(() => {
          if (isLogged) {
            loadCities();
          }
        });
    }
  }, [
    countries,
    countriesCities,
    countriesCities.length,
    isLogged,
    loadCities,
    setError,
    townState.city,
    townState.country,
    user.countryId,
  ]);

  const formWrapperStyles = `${styles["form-wrapper"]} d-flex justify-content-center align-items-center`;
  const currentCities = countriesCities.find(
    (cc) =>
      cc.countryId === parseInt(user?.country?.countryId || user.countryId)
  );

  const areLoadedCities = !!currentCities;
  if (areLoadedCities && isLogged) {
    // fix deleting values
    currentCities.cities = currentCities.cities.filter(
      (c) => c.cityId !== townState.city[0].cities[0].cityId
    );
    currentCities.cities[0] = townState.city[0].cities[0];
  }
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
                defaultValue={user.email}
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
              defaultValue={user.userName}
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
              defaultValue={user.birthdate}
              onChange={onInputChange}
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
          <Form.Group className="form-group mb-3" controlId="confirmPassword">
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
                      isLogged ? g.id === user.gender.id : g.id === 1
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
          {!!parseInt(user?.country?.countryId || user.countryId) && (
            <Form.Group className="form-group mb-3" controlId="cityId">
              <Form.Label>Choose city</Form.Label>
              <Form.Select
                className="mb-3"
                name="cityId"
                onChange={onInputChange}
                onClick={loadCities}
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
              defaultValue={user.bio}
              placeholder="Enter your bio"
              onChange={onInputChange}
              required
            />
          </Form.Group>
          <Form.Group className="form-group mb-3" controlId="profilePicture">
            <Form.Label>Upload your profile picture</Form.Label>
            <Form.Control
              type="file"
              name="image"
              onChange={onInputChange}
              accept=".jpg,.jpeg,.png"
            />
          </Form.Group>
          <Form.Group className="form-group mb-3" controlId="pictures">
            <Form.Label>Upload your photos</Form.Label>
            <Form.Control
              type="file"
              name="photos"
              multiple
              onChange={onInputChange}
              accept=".jpg,.jpeg,.png"
            />
          </Form.Group>
          {error && (
            <div className="text-danger mb-3">
              {error.split("\n").map((message, key) => {
                return <div key={key}>{message}</div>;
              })}
            </div>
          )}
          <Button variant="primary" type="submit">
            {isLogged ? "Edit" : "Register"}
          </Button>
        </Form>
      </div>
    </div>
  );
}
