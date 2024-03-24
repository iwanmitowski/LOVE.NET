import Spinner from "react-bootstrap/Spinner";

import styles from "./Loader.module.css";

export default function Loader() {
  return (
    <div className={styles["loading"]}>
      <Spinner
        variant="danger"
        animation="border"
        role="status"
        style={{ width: "4rem", height: "4rem" }}
      >
        <span className="visually-hidden" style={{ zIndex: "10000" }}>
          Loading...
        </span>
      </Spinner>
    </div>
  );
}
