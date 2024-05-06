import { useEffect, useState } from "react";
import ChatRoomCard from "./ChatRoomCard";

import * as chatService from "../../services/chatService";
import { useChat } from "../../hooks/useChat";
import { useIdentityContext } from "../../hooks/useIdentityContext";
import ChatRoom from "./ChatRoom";

export default function ChatRooms() {
  const { user } = useIdentityContext();
  const [rooms, setRooms] = useState([]);
  const chatState = useChat();
  const [chat, setChat] = useState([]);
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
        stopConnection={chatState.stopConnection}
        fetchMessages={fetchMessages}
        onHide={() => onCloseChat()}
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
            chatState.setUserConnection({ userId: user.id, roomId: r.id, profilePictureUrl: user.profilePicture, userName: user.userName });
          }}
        />
      ))}
    </div>
  );
}
