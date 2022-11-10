/* eslint-disable jsx-a11y/alt-text */
import { Button } from "react-bootstrap";
import styles from "./Images.module.css";

export default function ImageCard(props) {
  const imageUrl = props.imageUrl;

  return (
    <div className={`card flex-sm-fill col-md-3 ${styles.card}`}>
      <img className="card-img-top" src={imageUrl} />
      <div class="card-body d-flex  justify-content-center">
        <Button className={styles.button} variant="primary">Make as profile picture</Button>
        <Button className={styles.button} variant="danger">X</Button>
      </div>
    </div>
  );
}
