import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";

import * as identityService from "../../../services/identityService";
import * as countryService from "../../../services/countryService";
import * as genderService from "../../../services/genderService";
import * as date from "../../../utils/date.js";

import styles from "../Auth.module.css";

export default function Register() {
  const navigate = useNavigate();

  const [user, setUser] = useState({
    email: "",
    password: "",
    confirmPassword: "",
    userName: "",
    bio: "bio",
    birthdate: date.getLatestLegal().toISOString().split("T")[0],
    countryId: 0,
    cityId: 0,
    genderId: 1,
    image: null,
    photos: [],
  });

  const [countries, setCountries] = useState([
    {
      countryId: 0,
      countryName: "Chooce country here",
    },
  ]);

  const [countriesCities, setCities] = useState([
    {
      countryId: 0,
      cities: [
        {
          cityId: 0,
          cityName: "Choose city here",
        },
      ],
    },
  ]);

  const [genders, setGenders] = useState([]);

  const [error, setError] = useState("");

  useEffect(() => {
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
        });
      genderService.getAll().then((res) => {
        setGenders(res);
      });
    }
  }, [countries]);

  useEffect(() => {
    const isCountryFetched = countriesCities.some(
      (c) => c.countryId === parseInt(user.countryId)
    );
    if (user.countryId !== 0 && !isCountryFetched) {
      countryService.getCitiesByCountryId(user.countryId).then((res) => {
        setCities((prevState) => {
          return [...prevState, res];
        });
      });
    }
  }, [user.countryId, countriesCities]);

  const onInputChange = (e) => {
    setUser((prevState) => {
      let currentName = e.target.name;
      let currentValue = e.target.value;

      if (currentName === "image") {
        return {
          ...prevState,
          [currentName]: e.target.files[0],
        };
      }

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
      .register(user)
      .then(() => {
        navigate(`/verify?email=${user.email}`);
      })
      .catch((error) => {
        setError(error.message);
      });
  };

  const formWrapperStyles = `${styles["form-wrapper"]} d-flex justify-content-center align-items-center`;
  const currentCities = countriesCities.find(
    (cc) => cc.countryId === parseInt(user.countryId)
  );
  const areLoadedCities = !!currentCities;

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
                    defaultChecked={g.id === 1}
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
              {countries.map((c) => {
                return (
                  <option key={c.countryId} value={c.countryId}>
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
              >
                {areLoadedCities &&
                  currentCities.cities.map((c) => {
                    return (
                      <option key={c.cityId + c.cityName} value={c.cityId}>
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
    </div>
  );
}
