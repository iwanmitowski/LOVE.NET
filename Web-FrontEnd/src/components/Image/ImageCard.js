/* eslint-disable jsx-a11y/alt-text */
import { Button } from "react-bootstrap";
import styles from "./Images.module.css";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faStar, faTrash } from "@fortawesome/free-solid-svg-icons";

export default function ImageCard(props) {
  const imageUrl = props.imageUrl;
  const imageId = props.imageId;
  const isPfp = props.isPfp;
  const removeImageFromList = props.removeImageFromList;
  const setNewProfilePicture = props.setNewProfilePicture;

  return (
    <div className={`card flex-sm-fill col-md-3 ${styles.card}`}>
      {!isPfp && (
        <div
          className="d-flex justify-content-between w-100 px-2 position-absolute"
          style={{
            bottom: "4px",
          }}
        >
          <FontAwesomeIcon
            className={styles["icon-action-button"]}
            icon={faStar}
            onClick={() => setNewProfilePicture(imageId)}
          />
          <FontAwesomeIcon
            className={styles["icon-action-button"]}
            icon={faTrash}
            onClick={() => removeImageFromList(imageId)}
          />
        </div>
      )}
      <img
        className={`card-img img-fluid ${styles["card-image-size"]}`}
        src={imageUrl}
      />
    </div>
  );
}
