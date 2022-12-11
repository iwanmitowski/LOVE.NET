import { useEffect, useState, Fragment } from "react";
import { useNavigate } from "react-router-dom";
import { useIdentityContext } from "../hooks/useIdentityContext";
import SwipingCardContainer from "./SwipingCard/SwipingCardContainer";
import ChatModal from "./Modals/Chat/ChatModal";

import * as datingService from "../services/datingService";
import * as chatService from "../services/chatService";

import { useChat } from "../hooks/useChat";
import Loader from "./Shared/Loader/Loader";

export default function Matches() {
  const navigate = useNavigate();
  const { user, isLogged, userLogout } = useIdentityContext();
  const chatState = useChat();
  const [hasMore, setHasMore] = useState(true);
  const [matches, setMatches] = useState([]);
  const [chatUser, setChatUser] = useState();
  const [chat, setChat] = useState([]);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    setChat(() => [...chatState.messages]);
  }, [chatState.messages]);

  useEffect(() => {
    if (chatUser) {
      chatState.setUserConnection({ userId: user.id, roomId: chatUser.roomId });
    }
  }, [chatUser]);

  const fetchUsers = () => {
    if (hasMore) {
      if (matches.length === 0) {
        setIsLoading(() => true);
      }
      
      const page = Math.floor(matches.length / 10) + 1;

      datingService
        .getMatches({ userId: user.id, page })
        .then((res) => {
          setMatches((prevState) => {
            const currentMatches = [...prevState, ...res.matches];
            const hasMore = currentMatches.length < res.totalMatches;
            setHasMore(hasMore);
            return currentMatches;
          });
        })
        .catch((error) => {
          if (
            error?.response?.status === 401 ||
            error?.message?.includes("status code 401")
          ) {
            userLogout();
          } else if (
            error?.response?.status === 403 ||
            error?.message?.includes("status code 403")
          ) {
            navigate("/forbidden");
          } else if (
            error?.response?.status === 404 ||
            error?.message?.includes("status code 404")
          ) {
            navigate("/notfound");
          }
        })
        .finally(() => setIsLoading(() => false));
    }
  };

  useEffect(() => {
    if (isLogged && matches.length === 0) {
      fetchUsers();
    }
  }, [isLogged]);

  const onCloseChat = () => {
    chatState.stopConnection().then(() => {
      setChatUser(null);
    });
  };

  const fetchMessages = () => {
    if (chatState.hasMoreMessagesToLoad) {
      chatService
        .getChat(
          chatUser.roomId,
          Math.floor(chatState.messages.length / 10) + 1
        )
        .then((res) => {
          chatState.setMessages((prevState) => {
            const currentMessages = [...prevState, ...res.messages];
            const hasMore = currentMessages.length < res.totalMessages;
            chatState.setHasMoreMessagesToLoad(hasMore);
            return currentMessages;
          });
        });
    }
  };
  return isLoading ? (
    <Loader />
  ) : (
    <Fragment>
      <ChatModal
        show={!!chatUser}
        onHide={() => onCloseChat()}
        user={chatUser}
        chat={chat}
        sendMessage={chatState.sendMessage}
        fetchMessages={fetchMessages}
      />
      <SwipingCardContainer
        fetchUsers={fetchUsers}
        hasMoreUsersToLoad={hasMore}
        users={matches}
        startChat={setChatUser}
      />
    </Fragment>
  );
}
