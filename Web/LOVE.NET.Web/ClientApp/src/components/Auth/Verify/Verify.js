import { Fragment, useState } from "react";
import { useEffect } from "react";
import { Button } from "react-bootstrap";
import { Link, useSearchParams } from "react-router-dom";

import * as emailService from "../../../services/emailService";
import styles from "../../Shared/Forms.module.css";

export default function Verify() {
  const [searchParams] = useSearchParams();

  const [message, setMessage] = useState("");

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
    emailService.resend(email).catch((error) => {
      const [message] = error.response.data;
      setMessage(() => message);
    });
  };

  const formWrapperStyles = `${styles["form-wrapper"]} d-flex justify-content-center align-items-center`;

  const content =
    isVerifying || message
      ? message
      : "We have sent you an verifiaction email. Check your email";

  const button = isVerifying ? (
    <Link className="nav-link" to="/login">
      here
    </Link>
  ) : (
    <Fragment>
      <div>Haven't received our email ?</div>
      <Button variant="primary" type="submit" onClick={resendEmail}>
        Resend
      </Button>
    </Fragment>
  );

  return (
    <div className={formWrapperStyles}>
      <div className={styles["input-fields-length"]}>
        <h2>{content}</h2>
        {button}
      </div>
    </div>
  );
}
