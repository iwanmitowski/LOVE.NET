import { Button, Modal } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import SwipingCardCarousel from "../../SwipingCard/SwipingCardCarousel";

import styles from "../../SwipingCard/SwipingCard.module.css";

export default function MatchModal(props) {
  const navigate = useNavigate();
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
          <h4 className="text-uppercase m-0">Match</h4>
        </Modal.Title>
      </Modal.Header>
      {user && (
        <Modal.Body className="p-0">
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

      <Modal.Footer className="d-flex align-items-center justify-content-between">
        <Button onClick={onHide} className="btn btn-danger">Close</Button>
        <Button onClick={() => navigate("/matches")} className="btn btn-dark">To matches</Button>
      </Modal.Footer>
    </Modal>
  );
}
