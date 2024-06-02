import { useEffect, useState } from "react";
import ChatRoomCard from "./ChatRoomCard";

import * as chatService from "../../services/chatService";
import { useChat } from "../../hooks/useChat";
import { useIdentityContext } from "../../hooks/useIdentityContext";
import ChatRoom from "./ChatRoom";
import MatchModal from "../Modals/Match/MatchModal";

import * as datingService from "../../services/datingService";

export default function ChatRooms() {
  const { user, userLogout } = useIdentityContext();
  const [rooms, setRooms] = useState([]);
  const chatState = useChat();
  const [chat, setChat] = useState([]);
  const [matchModel, setMatchModel] = useState({
    isMatch: false,
  });
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    chatService.getChatrooms().then((res) => setRooms(res));
  }, []);

  useEffect(() => {
    setChat(() => [...chatState.messages]);
  }, [chatState.messages]);

  const onCloseChat = () => {
    chatState.stopConnection().then(() => {
      chatState.setUserConnection(null);
    });
  };

  const likeUser = (likedUserId) => {
    datingService
      .likeUser(likedUserId)
      .then((res) => {
        setMatchModel(res);
      })
      .catch((error) => {
        if (
          error?.response?.status === 401 ||
          error?.message?.includes("status code 401")
        ) {
          userLogout();
        } else {
          console.log(error);
        }
      });
  };

  const fetchMessages = (roomId) => {
    if (chatState.hasMoreMessagesToLoad) {
      chatService
        .getChat(roomId, Math.floor(chatState.messages.length / 10) + 1)
        .then((res) => {
          if (res.messages) {
            chatState.setMessages((prevState) => {
              const currentMessages = [...prevState, ...res.messages];
              const hasMore = currentMessages.length < res.totalMessages;
              chatState.setHasMoreMessagesToLoad(hasMore);
              return currentMessages;
            });
          }
        });
    }
  };

  if (chatState.userConnection) {
    return (
      <ChatRoom
        roomId={chatState.userConnection.roomId}
        chat={chat}
        usersInRoom={chatState.usersInRoom || []}
        sendMessage={chatState.sendMessage}
        fetchMessages={fetchMessages}
        onHide={() => onCloseChat()}
        likeUser={likeUser}
      />
    );
  }

  return (
    <div className="d-flex flex-wrap justify-content-center">
      {rooms.map((r) => (
        <ChatRoomCard
          id={r.id}
          title={r.title}
          imgSrc={r.url}
          join={() => {
            chatState.setUserConnection({
              userId: user.id,
              roomId: r.id,
              profilePictureUrl: user.profilePicture,
              userName: user.userName,
            });
          }}
        />
      ))}
      <MatchModal
        show={matchModel.isMatch}
        user={matchModel.user}
        onHide={() => setMatchModel({ isMatch: false })}
      />
    </div>
  );
}
