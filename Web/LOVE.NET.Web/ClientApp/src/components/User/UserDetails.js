/* eslint-disable react-hooks/exhaustive-deps */
import { useState, useEffect, Fragment } from "react";
import { useParams } from "react-router-dom";
import UserForm from "../User/UserForm";
import ImagesContainer from "../Image/ImagesContainer";

import * as identityService from "../../services/identityService";
import * as countryService from "../../services/countryService";
import * as genderService from "../../services/genderService";
import * as date from "../../utils/date";

export default function UserDetails() {
  const params = useParams();

  const userInitialState = {
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
    images: [],
    newImages: [],
  };

  const [user, setUser] = useState(userInitialState);
  const errorState = useState("");
  const [, setError] = errorState;
  const [genders, setGenders] = useState([]);
  const [countries, setCountries] = useState([]);
  const [userImages, setUserImages] = useState([]);

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
        setUserImages(accountPromiseResult.images);
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

      if (currentName === "newImages") {
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
      .editAccount(user)
      .then((res) => {
        setUser({
          ...res,
        });
        setUserImages(res.images);
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
      <h1>Your images</h1>
      {!!user?.images && <ImagesContainer images={userImages} />}
    </Fragment>
  );
}
