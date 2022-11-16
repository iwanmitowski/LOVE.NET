/* eslint-disable react-hooks/exhaustive-deps */
import { useState, Fragment } from "react";
import { Link, useNavigate } from "react-router-dom";
import UserForm from "../../User/UserForm";

import * as genderService from "../../../services/genderService";
import * as countryService from "../../../services/countryService";
import * as identityService from "../../../services/identityService";
import * as date from "../../../utils/date.js";
import { useEffect } from "react";

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
    newImages: [],
  });

  const [genders, setGenders] = useState([]);
  const [countries, setCountries] = useState([]);
  const errorState = useState("");
  const [, setError] = errorState;

  useEffect(() => {
    countryService
      .getAll()
      .then((res) => {
        setCountries(res);
      })
      .catch((error) => {
        setError(error.message);
      });

    genderService.getAll().then((res) => setGenders(res));
  }, []);

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

      if (currentName === 'newImages') {
        return {
          ...prevState,
          [currentName]: e.target.files,
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

  return (
    <Fragment>
      <UserForm
        user={user}
        genders={genders}
        countries={countries}
        onFormSubmit={onFormSubmit}
        onInputChange={onInputChange}
        errorState={errorState}
      />
      <div className="mt-3">
        <p>
          Already have account ?
          <Link className="nav-link" to="/login">
            Login
          </Link>
        </p>
      </div>
    </Fragment>
  );
}
