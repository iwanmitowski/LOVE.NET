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

  const removeImageFromList = (id) => {
    setUser((prevState) => {
      const filteredImages = prevState.images.filter((i) => i.id !== id);

      return {
        ...prevState,
        images: filteredImages,
      };
    });
  };

  const setNewProfilePicture = (id) => {
    setUser((prevState) => {
      const oldPfpIndex = prevState.images.findIndex((i) => i.isProfilePicture);
      const newPfpIndex = prevState.images.findIndex((i) => i.id === id);

      const oldPfp = { ...prevState.images[oldPfpIndex], isProfilePicture: false };
      const newPfp = { ...prevState.images[newPfpIndex], isProfilePicture: true };

      const filteredImages = [...prevState.images];
      filteredImages[oldPfpIndex] = newPfp;
      filteredImages[newPfpIndex] = oldPfp;

      return {
        ...prevState,
        images: filteredImages,
      };
    });
  };

  const onFormSubmit = (e) => {
    e.preventDefault();

    identityService
      .editAccount(user)
      .then((res) => {
        setUser({
          ...res,
        });
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
      {!!user?.images && (
        <ImagesContainer
          images={user.images}
          removeImageFromList={removeImageFromList}
          setNewProfilePicture={setNewProfilePicture}
        />
      )}
    </Fragment>
  );
}
