import { Modal } from "react-bootstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPaperPlane, faTimes } from "@fortawesome/free-solid-svg-icons";
import { useState } from "react";
import { useIdentityContext } from "../../../hooks/useIdentityContext";
import HomeMessage from "./Messages/HomeMessage";
import AwayMessage from "./Messages/AwayMessage";
import { useChat } from "../../../hooks/useChat";
import InfiniteScroll from "react-infinite-scroll-component";

import styles from "./ChatModal.module.css";
import { Fragment } from "react";

export default function ChatModal(props) {
  const { user } = useIdentityContext();
  const { hasMoreMessagesToLoad } = useChat();
  const defaultMessage = {
    text: "",
    image: null,
  };

  const [pastedImageUrl, setPastedImageUrl] = useState("");
  const [currentMessage, setCurrentMessage] = useState(defaultMessage);

  const currentUser = props.user;
  const sendMessage = props.sendMessage;
  const chat = props.chat;
  const fetchMessages = props.fetchMessages;

  const profilePicture = currentUser?.images?.find(
    (i) => i.isProfilePicture
  )?.url;

  const onSendingMessage = (e) => {
    e.preventDefault();

    if (!currentMessage.text && !currentMessage.image) {
      return;
    }

    sendMessage({
      roomId: currentUser.roomId,
      userId: user.id,
      text: currentMessage.text,
    });

    if (currentMessage.image) {
      sendMessage({
        roomId: currentUser.roomId,
        userId: user.id,
        image: currentMessage.image,
      });
    }

    setCurrentMessage(defaultMessage);
    setPastedImageUrl("");
  };

  const onInputChange = (e) => {
    let currentValue = e.target.value;
    setCurrentMessage((prevStae) => {
      return {
        ...prevStae,
        text: currentValue,
      };
    });
  };

  const onImagePaste = (e) => {
    const clipboardItems = e.clipboardData.items;
    const items = [].slice.call(clipboardItems).filter(function (item) {
      // Filter the image items only
      return /^image\//.test(item.type);
    });

    if (items.length === 0) {
      return;
    }

    const item = items[0];
    const blob = item.getAsFile();
    setPastedImageUrl(URL.createObjectURL(blob));

    let file = new File(
      [blob],
      item.name,
      {
        type: item.type,
        lastModified: new Date().getTime(),
      },
      "utf-8"
    );

    getBase64(file).then((res) => {
      let text = res;
      let skipCharactersCount = text.indexOf(",") + 1;
      text = text.slice(skipCharactersCount);

      setCurrentMessage((prevStae) => {
        return {
          ...prevStae,
          image: text,
        };
      });
    });
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
        <Modal.Title id="contained-modal-title-vcenter"></Modal.Title>
      </Modal.Header>
      {currentUser && (
        <Modal.Body>
          <div className="container py-5 px-4">
            <div className="row rounded-lg">
              <div className="col-12 px-0">
                <div
                  id="scrollableDiv"
                  className={`px-4 ${styles["chat-box"]} bg-white`}
                >
                  <InfiniteScroll
                    dataLength={chat.length}
                    next={fetchMessages}
                    style={{ display: "flex", flexDirection: "column-reverse" }} //To put endMessage and loader to the top.
                    inverse={true}
                    hasMore={hasMoreMessagesToLoad}
                    scrollableTarget="scrollableDiv"
                  >
                    {chat.filter(m => !m.isSystemMessage).map((message, index) => (
                      <Fragment>
                        {message.imageUrl && (
                          <img
                            key={index + 1}
                            src={message.imageUrl}
                            alt="pastedImage"
                            className="form-control rounded-0 border-0 bg-light"
                          />
                        )}
                        {message.text &&
                          (message.userId === user.id ? (
                            <HomeMessage key={index + 1} message={message} />
                          ) : (
                            <AwayMessage
                              key={index + 1}
                              message={message}
                              profilePicture={profilePicture}
                            />
                          ))}
                      </Fragment>
                    ))}
                  </InfiniteScroll>
                </div>
                <form className="bg-light" onSubmit={onSendingMessage}>
                  <div className="input-group">
                    {pastedImageUrl && (
                      <Fragment>
                        <img
                          src={pastedImageUrl}
                          alt="pastedImage"
                          id="preview"
                          className="form-control rounded-0 border-0 bg-light"
                          style={{
                            height: "180px",
                            padding: "0",
                          }}
                        />
                        <div className="input-group-append">
                          <button
                            onClick={() => setPastedImageUrl("")}
                            className="btn btn-link"
                          >
                            {" "}
                            <FontAwesomeIcon icon={faTimes} className="p-2" />
                          </button>
                        </div>
                      </Fragment>
                    )}
                  </div>
                  <div className="input-group">
                    <input
                      type="text"
                      placeholder="Type a message"
                      aria-describedby="button-addon2"
                      className="form-control rounded-0 border-0 bg-light"
                      value={currentMessage.text}
                      onChange={onInputChange}
                      onPasteCapture={onImagePaste}
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

function getBase64(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => resolve(reader.result);
    reader.onerror = (error) => reject(error);
  });
}
