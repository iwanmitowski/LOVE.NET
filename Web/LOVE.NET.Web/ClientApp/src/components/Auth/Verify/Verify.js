import { useState } from "react";
import { useEffect } from "react";
import { Link, useSearchParams } from "react-router-dom";

import * as emailService from "../../../services/emailService";
import styles from "../Auth.module.css";

export default function Verify() {
  const [searchParams] = useSearchParams();

  const [message, setMessage] = useState("");

  const token = searchParams.get("token");
  const email = searchParams.get("email");

  useEffect(() => {
    emailService.verify(token, email).then((res) => {
      setMessage(() => res);
    });
  }, []);

  const formWrapperStyles = `${styles["form-wrapper"]} d-flex justify-content-center align-items-center`;

  return (
    <div className={formWrapperStyles}>
      <div className={styles["input-fields-length"]}>
        <h2>
          {message}{" "}
          <Link className="nav-link" to="/login">
            here
          </Link>
        </h2>
      </div>
      <div></div>
    </div>
  );
}
