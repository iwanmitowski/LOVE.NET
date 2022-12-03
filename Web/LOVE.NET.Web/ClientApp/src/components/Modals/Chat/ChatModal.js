import { Modal } from "react-bootstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPaperPlane } from "@fortawesome/free-solid-svg-icons";
import styles from "./ChatModal.module.css";
import { useState, useRef } from "react";
import { useIdentityContext } from "../../../hooks/useIdentityContext";
import HomeMessage from "./Messages/HomeMessage";
import AwayMessage from "./Messages/AwayMessage";
import { useEffect } from "react";

export default function ChatModal(props) {
  const { user } = useIdentityContext();
  const [currentMessage, setCurrentMessage] = useState("");

  const currentUser = props.user;
  const sendMessage = props.sendMessage;
  const chat = props.chat;

  const profilePicture = currentUser?.images?.find(
    (i) => i.isProfilePicture
  )?.url;

  const onSendingMessage = (e) => {
    e.preventDefault();

    if (!currentMessage) {
      return;
    }

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
                <div
                  ref={scrollRef}
                  className={`px-4 ${styles["chat-box"]} bg-white`}
                >
                  {chat.map((message, index) =>
                    message.userId === user.id ? (
                      <HomeMessage key={index + 1} message={message} />
                    ) : (
                      <AwayMessage
                        key={index + 1}
                        message={message}
                        profilePicture={profilePicture}
                      />
                    )
                  )}
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
