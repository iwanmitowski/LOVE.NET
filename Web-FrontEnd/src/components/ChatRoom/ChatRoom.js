import {
  Button,
  Card,
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
import { useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faChevronCircleLeft,
  faChevronLeft,
  faPaperPlane,
  faTimes,
} from "@fortawesome/free-solid-svg-icons";
import InfiniteScroll from "react-infinite-scroll-component";

import styles from "../Modals/Chat/ChatModal.module.css";

export default function ChatRoom(props) {
  const { user } = useIdentityContext();
  const chatState = useChat();

  const defaultMessage = {
    text: "",
    image: null,
  };
  const [pastedImageUrl, setPastedImageUrl] = useState("");
  const [currentMessage, setCurrentMessage] = useState(defaultMessage);

  const sendMessage = props.sendMessage;
  const chat = props.chat;
  const fetchMessages = props.fetchMessages;
  const roomId = props.roomId;
  const usersInRoom = props.usersInRoom;

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
      <Row className="vh-100">
        <Col>
          <div className="d-flex align-items-center">
            <FontAwesomeIcon
              icon={faChevronLeft}
              style={{ marginRight: "4px" }}
            />
            <span>Rooms</span>
          </div>
        </Col>
        <Col xs={9} className="d-flex flex-column border-end">
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
                        profilePicture={message.profilePicture}
                      />
                    ))}
                </>
              ))}
            </InfiniteScroll>
          </div>
          <Form className="d-flex flex-column" onSubmit={onSendingMessage}>
            <div className="input-group">
              {pastedImageUrl && (
                <>
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
                  <FontAwesomeIcon icon={faPaperPlane} className="me-2" />
                </button>
              </div>
            </div>
          </Form>
        </Col>
        <Col xs={2}>
          <ListGroup variant="flush">
            {usersInRoom.map((user) => (
              <ListGroup.Item
                key={user.id}
                className="d-flex align-items-center"
              >
                <Image
                  src={user.profilePictureUrl}
                  roundedCircle
                  width="24"
                  height="24"
                />
                <span className="ms-2">{user.userName}</span>
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
