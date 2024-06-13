import { Link } from "react-router-dom";
import styles from "../../../Shared/Forms.module.css";

export default function Forbidden() {
  const formWrapperStyles = `${styles["form-wrapper"]} d-flex justify-content-center align-items-center`;
  return (
    <div className={formWrapperStyles}>
      <div className="bg-light rounded shadow p-3">
        <div className={styles["input-fields-length"]}>
          <h3>Forbidden! You can't do this</h3>
          <Link className="nav-link" to="/LOVE.NET">
            Take me back
          </Link>
        </div>
      </div>
    </div>
  );
}
