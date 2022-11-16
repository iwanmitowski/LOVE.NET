/* eslint-disable jsx-a11y/alt-text */
import { Fragment } from "react";
import { Button } from "react-bootstrap";
import styles from "./Images.module.css";

export default function ImageCard(props) {
  const imageUrl = props.imageUrl;
  const imageId = props.imageId;
  const isPfp = props.isPfp;
  console.log();
  return (
    <div className={`card flex-sm-fill col-md-3 ${styles.card}`}>
      <h1>
        <span>{`${imageId} ${isPfp}`}</span>
      </h1>
      <img className="card-img-top" src={imageUrl} />
      <div className="card-body d-flex  justify-content-center">
        {!isPfp && (
          <Fragment>
            <Button className={styles.button} variant="primary">
              Set new profile picture
            </Button>
            <Button className={styles.button} variant="danger">
              X
            </Button>
          </Fragment>
        )}
      </div>
    </div>
  );
}
