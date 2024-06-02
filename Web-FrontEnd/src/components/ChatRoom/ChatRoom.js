import {
  Button,
  Col,
  Container,
  Form,
  Image,
  ListGroup,
  Row,
} from "react-bootstrap";
import AwayMessage from "../Modals/Chat/Messages/AwayMessage";
import HomeMessage from "../Modals/Chat/Messages/HomeMessage";
import { useIdentityContext } from "../../hooks/useIdentityContext";
import { useChat } from "../../hooks/useChat";
import { useEffect, useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faChevronLeft,
  faHeart,
  faPaperPlane,
  faTimes,
  faWarning,
} from "@fortawesome/free-solid-svg-icons";
import InfiniteScroll from "react-infinite-scroll-component";

import styles from "../Modals/Chat/ChatModal.module.css";
import { chatConstants } from "../../utils/constants";

export default function ChatRoom(props) {
  const { user } = useIdentityContext();
  const chatState = useChat();

  const defaultMessage = {
    text: "",
    image: null,
  };
  const [pastedImageUrl, setPastedImageUrl] = useState("");
  const [currentMessage, setCurrentMessage] = useState(defaultMessage);
  const [showSensitiveDataWarning, setShowSensitiveDataWarning] =
    useState(false);

  const sendMessage = props.sendMessage;
  const chat = props.chat;
  const fetchMessages = props.fetchMessages;
  const roomId = props.roomId;
  const usersInRoom = props.usersInRoom;
  const stopConnection = props.onHide;
  const likeUser = props.likeUser;

  useEffect(() => {
    isSendingSensitiveData();
  }, [currentMessage.text, pastedImageUrl]);

  useEffect(() => {
    window.addEventListener("beforeunload", () => {
      stopConnection();
    });
    return () => {
      stopConnection();
      window.removeEventListener("beforeunload", stopConnection);
    };
  }, []);

  const onSendingMessage = (e) => {
    e.preventDefault();

    if (!currentMessage.text && !currentMessage.image) {
      return;
    }

    sendMessage({
      roomId,
      userId: user.id,
      text: currentMessage.text,
      profilePicture: user.profilePicture,
    });

    if (currentMessage.image) {
      sendMessage({
        roomId,
        userId: user.id,
        image: currentMessage.image,
        profilePicture: user.profilePicture,
      });
    }

    setCurrentMessage(defaultMessage);
    setPastedImageUrl("");
  };

  const isSendingSensitiveData = () => {
    const matches = currentMessage?.text?.match(chatConstants.PHONE_REGEX);
    const hasTypedPhoneNumber = !!matches?.length;
    const isSendingImage = !!pastedImageUrl;
    setShowSensitiveDataWarning(isSendingImage || hasTypedPhoneNumber);
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
    <Container className="py-3">
      <Row>
        <Col>
          <div
            className="d-flex align-items-center"
            style={{ cursor: "pointer" }}
            onClick={stopConnection}
          >
            <FontAwesomeIcon
              icon={faChevronLeft}
              style={{ marginRight: "4px" }}
            />
            <span>Rooms</span>
          </div>
        </Col>
        <Col xs={8} className="d-flex flex-column border-end">
          <div
            id="scrollableDiv"
            className={`px-4 ${styles["chat-box"]} bg-white`}
            style={{ height: "80vh" }}
          >
            <InfiniteScroll
              dataLength={chat.length}
              next={fetchMessages}
              style={{ display: "flex", flexDirection: "column-reverse" }} //To put endMessage and loader to the top.
              inverse={true}
              hasMore={chatState.hasMoreMessagesToLoad}
              scrollableTarget="scrollableDiv"
            >
              {chat.map((message, index) => (
                <>
                  {message.imageUrl && (
                    <img
                      key={index + 1}
                      src={message.imageUrl}
                      alt="pastedImage"
                      style={{
                        height: "200px",
                        padding: "0",
                        objectFit: "contain",
                      }}
                      className="form-control my-4 rounded-0 border-0 bg-light"
                    />
                  )}
                  {message.text &&
                    (message.isSystemMessage ? (
                      <p>{message.text}</p>
                    ) : message.userId === user.id ? (
                      <HomeMessage key={index + 1} message={message} />
                    ) : (
                      <AwayMessage
                        key={index + 1}
                        message={message}
                        profilePicture={message.profilePicture}
                      />
                    ))}
                </>
              ))}
            </InfiniteScroll>
          </div>
          <Form className="d-flex flex-column" onSubmit={onSendingMessage}>
            {showSensitiveDataWarning && (
              <div
                className="py-2 bg-white d-flex justify-content-center align-items-center"
                style={{ color: "#ffa500" }}
              >
                <FontAwesomeIcon icon={faWarning} />
                <p className="my-0 mx-1">
                  You are probably going to send sensitive data!
                </p>
              </div>
            )}
            <div className="input-group">
              {pastedImageUrl && (
                <>
                  <img
                    src={pastedImageUrl}
                    alt="pastedImage"
                    id="preview"
                    className="form-control rounded-0 border-0 bg-light"
                    style={{
                      height: "200px",
                      padding: "0",
                      objectFit: "contain",
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
                </>
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
                  <FontAwesomeIcon icon={faPaperPlane} />
                </button>
              </div>
            </div>
          </Form>
        </Col>
        <Col xs={3}>
          <ListGroup variant="flush">
            {usersInRoom.map((userInRoom) => (
              <ListGroup.Item
                key={userInRoom.id}
                className="d-flex align-items-center justify-content-between"
              >
                <div>
                  <Image
                    src={userInRoom.profilePictureUrl}
                    roundedCircle
                    width="24"
                    height="24"
                  />
                  <span className="ms-2">{userInRoom.userName}</span>
                </div>

                {user.id !== userInRoom.id && (
                  <Button
                    className="m-0 p-0 rounded shadow"
                    style={{
                      border: "1px solid lightgray",
                    }}
                    title="Like"
                    variant="light"
                    type="submit"
                    onClick={() => likeUser(userInRoom.id)}
                  >
                    <FontAwesomeIcon
                      icon={faHeart}
                      style={{
                        width: "16px",
                        height: "16px",
                        display: "flex",
                        margin: "3px",
                        color: "#74B72E",
                      }}
                    />
                  </Button>
                )}
              </ListGroup.Item>
            ))}
          </ListGroup>
        </Col>
      </Row>
    </Container>
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
