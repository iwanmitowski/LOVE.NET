/* eslint-disable react-hooks/exhaustive-deps */
import { useState, useEffect, Fragment } from "react";
import { useNavigate, useParams } from "react-router-dom";
import UserForm from "../User/UserForm";
import ImagesContainer from "../Image/ImagesContainer";
import { useIdentityContext } from "../../hooks/useIdentityContext";

import * as identityService from "../../services/identityService";
import * as countryService from "../../services/countryService";
import * as genderService from "../../services/genderService";
import * as date from "../../utils/date";
import Loader from "../Shared/Loader/Loader";
import { Button, Col, Form } from "react-bootstrap";

export default function UserDetails() {
  const params = useParams();
  const navigate = useNavigate();
  const { userLogout, setUserLocation } = useIdentityContext();
  const [isLoading, setIsLoading] = useState(false);

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
  const [error, setError] = useState("");
  const [genders, setGenders] = useState([]);
  const [countries, setCountries] = useState([]);
  const [countriesCities, setCities] = useState([]);

  const userId = params.id;
  useEffect(() => {
    setIsLoading(() => true);
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
        setUserLocation({
          latitude: accountPromiseResult.latitude,
          longitude: accountPromiseResult.longitude,
        });
      })
      .catch((error) => {
        if (
          error?.response?.status === 401 ||
          error?.message?.includes("status code 401")
        ) {
          userLogout();
        } else if (
          error?.response?.status === 403 ||
          error?.message?.includes("status code 403")
        ) {
          navigate("/forbidden");
        } else if (
          error?.response?.status === 404 ||
          error?.message?.includes("status code 404")
        ) {
          navigate("/notfound");
        } else {
          setError(error.message);
        }
      })
      .finally(() => setIsLoading(() => false));
  }, [userId]);

  useEffect(() => {
    if (!!user.countryId) {
      countryService.getCitiesByCountryId(user.countryId).then((res) => {
        setCities((prevState) => {
          return [...prevState, res];
        });
      });
    }
  }, [user.countryId]);

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

      const oldPfp = {
        ...prevState.images[oldPfpIndex],
        isProfilePicture: false,
      };
      const newPfp = {
        ...prevState.images[newPfpIndex],
        isProfilePicture: true,
      };

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
        setUserLocation({
          latitude: res.latitude,
          longitude: res.longitude,
        });
      })
      .catch((error) => {
        if (
          error?.response?.status === 401 ||
          error?.message?.includes("status code 401")
        ) {
          userLogout();
        } else if (
          error?.response?.status === 403 ||
          error?.message?.includes("status code 403")
        ) {
          navigate("/forbidden");
        } else if (
          error?.response?.status === 404 ||
          error?.message?.includes("status code 404")
        ) {
          navigate("/notfound");
        } else {
          setError(error.message);
        }
      });
  };

  const currentCities = countriesCities.find(
    (cc) => cc.countryId === parseInt(user.countryId)
  );
  const areLoadedCities = !!currentCities;

  return isLoading ? (
    <Loader />
  ) : (
    <div className="d-flex justify-content-center mt-4">
      <div className="w-75 shadow-lg">
        <div className="container">
          <div className="row p-3">
            <div className="col col-5 pb-3 d-flex flex-column justify-content-center border-end border-secondary-subtle">
              <div className=" d-flex flex-column align-items-center justify-content-between border-bottom border-secondary-subtle p-2 pb-4">
                {!!user?.images && (
                  <img
                    className="rounded-circle mt-2 img-fluid object-fit-cover shadow profile-image"
                    alt="User profile"
                    style={{
                      width: "300px",
                      height: "300px",
                    }}
                    src={user.images[0]?.url}
                  />
                )}
              </div>
              <div className="lead ms-2">
                <div className="d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between">
                  <Col className="d-flex" sm={3}>
                    Email:
                  </Col>
                  <Col className="d-flex">
                    <span className="text-black">{user.email}</span>
                  </Col>
                </div>
                <div className="d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between">
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
                </div>
                <div className="d-flex fw-normal mt-2 text-black-50 justify-content-between">
                  <Col className="d-flex" sm={3}>
                    Birthday:
                  </Col>
                  <Col>
                    <span className="text-black">
                      <Form.Control
                        type="date"
                        name="birthdate"
                        value={new Date(user.birthdate).toLocaleDateString(
                          "en-CA",
                          {
                            year: "numeric",
                            month: "2-digit",
                            day: "2-digit",
                          }
                        )}
                        onChange={onInputChange}
                      />
                    </span>
                  </Col>
                </div>
                <div className="d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between">
                  <Col className="d-flex" sm={3}>
                    Gender:
                  </Col>
                  <Col className="d-flex">
                    <span className="text-black">
                      <div>
                        {genders.map((g) => {
                          return (
                            <Form.Check
                              inline
                              label={g.name}
                              name="genderId"
                              type="radio"
                              key={`${g.id}-${g.name}`}
                              defaultChecked={g.id === user.genderId}
                              value={g.id}
                              onChange={onInputChange}
                            />
                          );
                        })}
                      </div>
                    </span>
                  </Col>
                </div>
                <div className="d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between">
                  <Col className="d-flex" sm={3}>
                    Country:
                  </Col>
                  <Col className="d-flex">
                    <span className="text-black">
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
                    </span>
                  </Col>
                </div>
                {!!parseInt(user.countryId) && (
                  <div className="d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between">
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
                  </div>
                )}
                <div className="d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between">
                  <Col className="d-flex" sm={3}>
                    Photos:
                  </Col>
                  <Col className="d-flex">
                    <Form.Control
                      type="file"
                      name="newImages"
                      multiple
                      value={user?.newImages?.length ? null : ""}
                      onChange={onInputChange}
                      accept=".jpg,.jpeg,.png"
                    />
                  </Col>
                </div>
                <div className="d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between">
                  <Col className="d-flex" sm={3}>
                    Password:
                  </Col>
                  <Col className="d-flex">
                    <Form.Control
                      type="password"
                      name="password"
                      value={user.password || ""}
                      placeholder="Enter password"
                      onChange={onInputChange}
                      required
                    />
                  </Col>
                </div>
                <div className="d-flex fs-5 fw-normal mt-2 text-black-50 align-items-center justify-content-between">
                  <Col className="d-flex" sm={3}>
                    Confirm:
                  </Col>
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
                </div>
              </div>
              <div className="mt-3 ms-2 d-flex justify-content-start align-items-center">
                <Col className="d-flex" sm={4}>
                  <Button variant="dark" type="submit" onClick={onFormSubmit}>
                    Update profile
                  </Button>
                </Col>
                <Col>
                  {error && (
                    <div className="text-danger">
                      {error.split("\n").map((message, key) => {
                        return <div key={key}>{message}</div>;
                      })}
                    </div>
                  )}
                </Col>
              </div>
            </div>
            <div className="col col-7 d-flex flex-column pb-2 ps-4">
              <div className="fs-3 lead  mt-3">Hi, {user.userName}!</div>
              <div className="pt-3 mx-2 fs-5 text-break">
                <Form.Control
                  as="textarea"
                  name="bio"
                  rows={3}
                  value={user.bio}
                  placeholder="Enter your bio"
                  onChange={onInputChange}
                  required
                />
              </div>
              <div>
                {!!user?.images && (
                  <ImagesContainer
                    images={user.images}
                    removeImageFromList={removeImageFromList}
                    setNewProfilePicture={setNewProfilePicture}
                  />
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
