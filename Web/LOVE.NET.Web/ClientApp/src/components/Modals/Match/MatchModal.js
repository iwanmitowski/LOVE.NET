import { Button, Modal } from "react-bootstrap";
import SwipingCardCarousel from "../../SwipingCard/SwipingCardCarousel";

import styles from "../../SwipingCard/SwipingCard.module.css";

export default function MatchModal(props) {
  const onHide = props.onHide;
  const user = props.user;

  return (
    <Modal
      {...props}
      size="xs"
      aria-labelledby="contained-modal-title-vcenter"
      centered
    >
      <Modal.Header closeButton>
        <Modal.Title id="contained-modal-title-vcenter">
          <h1 className="text-success text-uppercase">It's a match !!!</h1>
        </Modal.Title>
      </Modal.Header>
      {user && (
        <Modal.Body>
          <div
            className={`card text-center ${styles["no-selecting"]}`}
            style={{ margin: "0px auto" }}
          >
            <SwipingCardCarousel
              images={user.images}
              styles={{ borderRadius: "0px !important" }}
            />
            <div className={`m-3 ${styles["card-body"]}`}>
              <p className={`card-text ${styles.userName}`}>
                <strong>{user.userName}</strong> {user.age}
              </p>
              <p className={`card-text ${styles.bio}`}>{user.bio}</p>
            </div>
            <ul className="list-group list-group-flush">
              <li className="list-group-item">{user.genderName}</li>
              <li className="list-group-item">{user.cityName}</li>
            </ul>
          </div>
        </Modal.Body>
      )}

      <Modal.Footer>
        <Button onClick={onHide}>Close</Button>
      </Modal.Footer>
    </Modal>
  );
}
