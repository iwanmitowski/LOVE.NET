/* eslint-disable jsx-a11y/alt-text */
import { Button } from "react-bootstrap";
import styles from "./Images.module.css";

export default function ImageCard(props) {
  const imageUrl = props.imageUrl;
  const imageId = props.imageId;
  const isPfp = props.isPfp;
  const removeImageFromList = props.removeImageFromList;
  const setNewProfilePicture = props.setNewProfilePicture;

  return (
    <div className={`card flex-sm-fill col-md-3 ${styles.card}`}>
      <img className="card-img-top" src={imageUrl} />
      <div className="card-body d-flex  justify-content-center">
        {!isPfp && (
          <div>
            <Button
              className={styles.button}
              variant="dark"
              onClick={() => setNewProfilePicture(imageId)}
            >
              Set new profile picture
            </Button>
            <Button
              className={styles.button}
              variant="danger"
              onClick={() => removeImageFromList(imageId)}
            >
              X
            </Button>
          </div>
        )}
      </div>
    </div>
  );
}
