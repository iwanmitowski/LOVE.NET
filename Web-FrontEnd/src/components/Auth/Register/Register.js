/* eslint-disable react-hooks/exhaustive-deps */
import { useState, Fragment } from "react";
import { useNavigate } from "react-router-dom";
import TaCModal from "../../Modals/TaCModal/TaCModal";

import * as genderService from "../../../services/genderService";
import * as countryService from "../../../services/countryService";
import * as identityService from "../../../services/identityService";
import * as date from "../../../utils/date.js";
import { useEffect } from "react";
import { Form } from "react-bootstrap";

import styles from "../../Shared/Forms.module.css";
import Step1 from "./Step1.js";
import Step2 from "./Step2.js";
import Step3 from "./Step3.js";
import Step4 from "./Step4.js";

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
    isReading: false,
    isAgreed: false,
  });

  const [genders, setGenders] = useState([]);
  const [countries, setCountries] = useState([]);
  const [error, setError] = useState("");
  const [step, setStep] = useState(1);

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

  const nextStep = () => setStep(step + 1);
  const prevStep = () => setStep(step - 1);

  const setIsReading = (isReading) => {
    setUser((prevState) => {
      return { ...prevState, isReading };
    });
  };

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

      if (currentName === "isAgreed") {
        return {
          ...prevState,
          isAgreed: !user.isAgreed,
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

    if (user.isAgreed) {
      identityService
        .register(user)
        .then(() => {
          navigate(`/verify?email=${user.email}`);
        })
        .catch((error) => {
          setError(error.message);
        });
    }
  };
  const formWrapperStyles = `${styles["form-wrapper"]} d-flex flex-column justify-content-center align-items-center`;
  return (
    <div className={formWrapperStyles}>
      <div className="bg-light rounded shadow p-3">
        <h1 className="pb-2">Register</h1>
        <div className={styles["input-fields-length"]}>
          <Form onSubmit={onFormSubmit}>
            {error && (
              <div className="text-danger mb-3">
                {error.split("\n").map((message, key) => {
                  return <div key={key}>{message}</div>;
                })}
              </div>
            )}
            {(() => {
              switch (step) {
                case 1:
                  return (
                    <Step1
                      user={user}
                      onInputChange={onInputChange}
                      nextStep={nextStep}
                    />
                  );
                case 2:
                  return (
                    <Step2
                      user={user}
                      genders={genders}
                      countries={countries}
                      onInputChange={onInputChange}
                      nextStep={nextStep}
                      prevStep={prevStep}
                    />
                  );
                case 3:
                  return (
                    <Step3
                      user={user}
                      setUser={setUser}
                      onInputChange={onInputChange}
                      nextStep={nextStep}
                      prevStep={prevStep}
                    />
                  );
                case 4:
                  return (
                    <Step4
                      user={user}
                      onInputChange={onInputChange}
                      setIsReading={setIsReading}
                      prevStep={prevStep}
                    />
                  );
                default:
                  return (
                    <>
                      <Step1
                        user={user}
                        onInputChange={onInputChange}
                        nextStep={nextStep}
                      />
                      <TaCModal
                        show={user.isReading}
                        onHide={() => setIsReading(false)}
                      ></TaCModal>
                    </>
                  );
              }
            })()}
          </Form>
        </div>
      </div>
    </div>
  );
}
