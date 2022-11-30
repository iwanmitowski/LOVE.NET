import { Modal } from "react-bootstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPaperPlane } from "@fortawesome/free-solid-svg-icons";
import styles from "./ChatModal.module.css";
import { useState } from "react";
import { useIdentityContext } from "../../../hooks/useIdentityContext";

export default function ChatModal(props) {
  const { user } = useIdentityContext();
  const [currentMessage, setCurrentMessage] = useState("");

  const currentUser = props.user;
  const sendMessage = props.sendMessage;

  const profilePicture = currentUser?.images?.find(
    (i) => i.isProfilePicture
  )?.url;

  const onSendingMessage = (e) => {
    e.preventDefault();

    sendMessage({
      roomId: currentUser.roomId,
      userId: user.id,
      text: currentMessage,
    });

    setCurrentMessage("");
  };

  const onInputChange = (e) => {
    let currentValue = e.target.value;
    setCurrentMessage(currentValue);
  };

  return (
    <Modal
      show={props.show}
      onHide={props.onHide}
      size="md"
      aria-labelledby="contained-modal-title-vcenter"
      centered
    >
      <Modal.Header closeButton>
        <Modal.Title id="contained-modal-title-vcenter">
          <h1 className="text-uppercase">{currentUser?.userName}</h1>
        </Modal.Title>
      </Modal.Header>
      {currentUser && (
        <Modal.Body>
          <div className="container py-5 px-4">
            <div className="row rounded-lg">
              <div className="col-12 px-0">
                <div className={`px-4 ${styles["chat-box"]} bg-white`}>
                  <div className={`${styles.media} w-75 mb-3`}>
                    <img
                      src={profilePicture}
                      alt="user"
                      width="50"
                      height="50"
                      className="rounded-circle"
                    />
                    <div className="media-body ms-3">
                      <div className="bg-light rounded py-2 px-3 mb-2">
                        <p
                          className={`${styles["text-small"]} mb-0 text-muted`}
                        >
                          Test which is a new approach all solutions
                        </p>
                      </div>
                      <p className="small text-muted">12:00 PM | Aug 13</p>
                    </div>
                  </div>

                  <div className={`${styles.media} w-50 ms-auto mb-3`}>
                    <div className="media-body">
                      <div className="bg-primary rounded py-2 px-3 mb-2">
                        <p className="text-small mb-0 text-white">
                          Test which is a new approach to have all solutions
                        </p>
                      </div>
                      <p className="small text-muted">12:00 PM | Aug 13</p>
                    </div>
                  </div>
                </div>

                <form className="bg-light" onSubmit={onSendingMessage}>
                  <div className="input-group">
                    <input
                      type="text"
                      placeholder="Type a message"
                      aria-describedby="button-addon2"
                      className="form-control rounded-0 border-0 bg-light"
                      value={currentMessage}
                      onChange={onInputChange}
                    />
                    <div className="input-group-append">
                      <button id="send" type="submit" className="btn btn-link">
                        {" "}
                        <FontAwesomeIcon icon={faPaperPlane} className="me-2" />
                      </button>
                    </div>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </Modal.Body>
      )}
    </Modal>
  );
}
