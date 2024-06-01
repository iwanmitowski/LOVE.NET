import { useEffect, useState } from "react";
import { Button, Form } from "react-bootstrap";
import { useSearchParams } from "react-router-dom";
import { useIdentityContext } from "../../../hooks/useIdentityContext";

import * as emailService from "../../../services/emailService";
import styles from "../../Shared/Forms.module.css";

export default function Reset() {
  const { isLogged, user } = useIdentityContext();
  const [searchParams] = useSearchParams();

  const token = searchParams.get("token");
  const userEmail = searchParams.get("email");

  const [data, setData] = useState({
    token: token,
    email: userEmail || "",
    password: "",
    confirmPassword: "",
  });
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    setMessage("");
    if (isLogged) {
      setData((prevState) => {
        return {
          ...prevState,
          email: user.email,
        };
      });
    }
  }, [isLogged]);

  const reset = async (e) => {
    e.preventDefault();

    emailService
      .resetPassword(data)
      .then((res) => {
        setError("");
        setMessage(() => res);
      })
      .catch((error) => {
        setError(error.message);
      });
  };

  const resendEmail = async (e) => {
    e.preventDefault();
    emailService
      .resendResetPasswordEmail(data.email)
      .then((res) => {
        setError("");
        setMessage(res);
      })
      .catch((error) => {
        setError(error.message);
      });
  };

  const onInputChange = (e) => {
    setData((prevState) => {
      let currentName = e.target.name;
      let currentValue = e.target.value;

      return {
        ...prevState,
        [currentName]: currentValue,
      };
    });

    setMessage("");
  };

  const formWrapperStyles = `${styles["form-wrapper"]} d-flex flex-column justify-content-center align-items-center`;

  return (
    <div className={formWrapperStyles}>
      <div
        className={`bg-light rounded shadow p-3 ${styles["input-fields-length"]}`}
      >
        {!isLogged && !data.token ? (
          <div>
            <h4 className="pb-2">Send reset password email</h4>
            <Form onSubmit={resendEmail}>
              <Form.Group className="form-group mb-3" controlId="email">
                <Form.Control
                  type="email"
                  name="email"
                  defaultValue={user?.email}
                  placeholder="Email"
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
                Send
              </Button>
            </Form>
          </div>
        ) : (
          <div>
            <h4>Reset password</h4>
            <Form onSubmit={reset}>
              <Form.Group className="form-group mb-3" controlId="password">
                <Form.Control
                  type="password"
                  name="password"
                  defaultValue={user?.password}
                  placeholder="Password"
                  onChange={onInputChange}
                  required
                />
              </Form.Group>
              <Form.Group
                className="form-group mb-3"
                controlId="confirmPassword"
              >
                <Form.Control
                  type="password"
                  name="confirmPassword"
                  value={data.confirmPassword || ""}
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
                Reset
              </Button>
            </Form>
          </div>
        )}
        {message && message}
      </div>
    </div>
  );
}
