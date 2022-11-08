import { useState, useEffect, Fragment } from "react";
import { useNavigate, useParams } from "react-router-dom";
import UserForm from "../User/UserForm";

import * as identityService from "../../services/identityService";
import * as countryService from "../../services/countryService";
import * as genderService from "../../services/genderService";
import * as date from "../../utils/date";

export default function UserDetails() {
  const navigate = useNavigate();
  const params = useParams();

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

  const errorState = useState("");
  const [, setError] = errorState;
  const [genders, setGenders] = useState([]);
  const [countries, setCountries] = useState([]);

  const userId = params.id;
  useEffect(() => {
    const promises = [
      identityService.getAccount(userId),
      genderService.getAll(),
      countryService.getAll(),
    ];

    Promise.all(promises)
      .then((results) => {
        const accountPromiseResult = results[0];
        const genderPromiseResult = results[1];
        const countryPromiseResult = results[2];

        setUser(accountPromiseResult);
        setGenders(genderPromiseResult);
        setCountries(countryPromiseResult);
      })
      .catch((error) => {
        setError(error.message);
      });
  }, [userId]);

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
      .register(user) // change ?
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
      <h1>Visualized images</h1>
    </Fragment>
  );
}
