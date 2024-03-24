import { Button, Form, Modal } from "react-bootstrap";
import { useState } from "react";

import * as dashboardService from "../../../services/dashboardService";

export default function ModerateModal(props) {
  const onHide = props.onHide;
  const userBanRequest = props.userBanRequest;
  const setUserBanRequest = props.setUserBanRequest;
  const [error, setError] = useState("");
  const onInputChange = (e) => {
    let currentValue = e.target.value;
    setUserBanRequest((prevState) => {
      return { ...prevState, bannedUntil: currentValue };
    });
  };

  const onFormSubmit = () => {
    dashboardService
      .moderateUser({
        ...userBanRequest,
        bannedUntil: isBan ? userBanRequest.bannedUntil : null,
      })
      .then(() => {
        setError("");
        onHide({
          id: userBanRequest.userId,
          isBanned: isBan,
        });
      })
      .catch((error) => {
        setError(error.message);
      });
  };

  const isBan = userBanRequest.isBan;

  return (
    <Modal
      show={props.show}
      onHide={() => {
        onHide();
        setError("");
      }}
      size="xs"
      aria-labelledby="contained-modal-title-vcenter"
      centered
    >
      <Modal.Header closeButton>
        <Modal.Title id="contained-modal-title-vcenter">
          <h6 className="text-danger text-uppercase">{`Are you sure you want to ${
            isBan ? "ban" : "unban"
          } that user ?`}</h6>
        </Modal.Title>
      </Modal.Header>
      {userBanRequest.user && isBan && (
        <Modal.Body>
          <Form>
            <Form.Group className="form-group mb-3" controlId="endDate">
              <Form.Label></Form.Label>
              <Form.Control
                type="date"
                name="endDate"
                value={new Date(userBanRequest.bannedUntil).toLocaleDateString(
                  "en-CA",
                  {
                    year: "numeric",
                    month: "2-digit",
                    day: "2-digit",
                  }
                )}
                onChange={onInputChange}
              />
            </Form.Group>
          </Form>
          {error && <h6 className="text-danger text-center">{error}</h6>}
        </Modal.Body>
      )}
      <Modal.Footer>
        <Button onClick={onFormSubmit}>{isBan ? "Ban" : "Unban"}</Button>
      </Modal.Footer>
    </Modal>
  );
}
