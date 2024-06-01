import { Fragment, useState, useEffect } from "react";
import { Button } from "react-bootstrap";
import { Link, useSearchParams } from "react-router-dom";

import * as emailService from "../../../services/emailService";
import styles from "../../Shared/Forms.module.css";

export default function Verify() {
  const [searchParams] = useSearchParams();

  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const token = searchParams.get("token");
  const email = searchParams.get("email");
  const isVerifying = !!token;

  useEffect(() => {
    if (isVerifying) {
      emailService.verify(token, email).then((res) => {
        setMessage(() => res);
      });
    }
  }, [isVerifying, email, token]);

  const resendEmail = async (e) => {
    e.preventDefault();
    emailService
      .resendVerifyEmail(email)
      .then((res) => {
        setError("");
        setMessage(res);
      })
      .catch((error) => {
        const [message] = error.response.data;
        setError(() => message);
      });
  };

  const formWrapperStyles = `${styles["form-wrapper"]} d-flex justify-content-center align-items-center`;

  const content =
    isVerifying || message
      ? message
      : "We have sent you an verifiaction email. Check your email";

  const button = (
    <Fragment>
      <h4>
        {isVerifying
          ? "Click below to verify your account"
          : "Haven't received our email?"}
      </h4>
      {isVerifying ? (
        <Link className="nav-link" to="/login">
          <Button variant="dark" type="submit" onClick={resendEmail}>
            Verify
          </Button>
        </Link>
      ) : (
        <>
          <Button variant="dark" type="submit" onClick={resendEmail}>
            Resend
          </Button>
        </>
      )}
    </Fragment>
  );
  return (
    <div className={formWrapperStyles}>
      <div className="bg-light rounded shadow p-3">
        <div className={styles["input-fields-length"]}>
          <h3>{content}</h3>
          {error && (
            <div className="text-danger mb-3">
              {error.split("\n").map((message, key) => {
                return <div key={key}>{message}</div>;
              })}
            </div>
          )}
          {button}
        </div>
      </div>
    </div>
  );
}
