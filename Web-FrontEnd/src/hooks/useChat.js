import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useState, useRef, useEffect } from "react";
import { globalConstants } from "../utils/constants";

import * as chatService from "../services/chatService";

export const useChat = () => {
  const connectionRef = useRef(null);
  const [messages, setMessages] = useState([]);
  const [usersInRoom, setUsersInRoom] = useState([]);
  const [hasMoreMessagesToLoad, setHasMoreMessagesToLoad] = useState(true);
  const [userConnection, setUserConnection] = useState(null);
  const latestMessages = useRef(null);

  latestMessages.current = messages;

  useEffect(() => {
    if (userConnection) {
      const newConnection = new HubConnectionBuilder()
        .withUrl(`${globalConstants.BASE_URL}/chat`)
        .configureLogging(LogLevel.Information)
        .withAutomaticReconnect()
        .build();

      newConnection
        .start()
        .then(() => {
          newConnection.invoke("JoinRoom", userConnection);
          newConnection.on("ReceiveMessage", (message) => {
            setMessages((prevState) => [message, ...prevState]);
          });
          newConnection.on("RefreshUsersList", (response) => {
            let changedUser;

            setUsersInRoom((prevState) => {
              changedUser = findChangedUser(
                prevState,
                response.users,
                response.hasLeft
              );
              return response.users;
            });

            setTimeout(() => {
              setMessages((prevState) => {
                if (!changedUser) {
                  return prevState;
                }

                const text = response.hasLeft
                  ? `User ${changedUser.userName} has left the chat.`
                  : `${
                      changedUser.id === userConnection.userId
                        ? "You"
                        : `User ${changedUser?.userName}`
                    } joined the chat. Say Hi!`;

                return [
                  {
                    text,
                    isSystemMessage: true,
                  },
                  ...prevState,
                ];
              });
            }, 0)
            
          });
        })
        .catch((error) => console.error("Connection failed: ", error));

      connectionRef.current = newConnection;

      chatService.getChat(userConnection.roomId).then((res) => {
        setMessages(res.messages);
        setHasMoreMessagesToLoad(res.messages.length < res.totalMessages);
      });
    }
  }, [userConnection]);

  const sendMessage = async (message) => {
    if (connectionRef.current) {
      try {
        await connectionRef.current.invoke("SendMessage", message);
      } catch (e) {
        console.log(e);
      }
    }
  };

  const stopConnection = async () => {
    await leaveRoom();
    if (connectionRef.current) {
      await connectionRef.current.stop();
      setMessages([]);
      connectionRef.current = null;
    }
  };

  const leaveRoom = async () => {
    if (connectionRef.current && userConnection) {
      try {
        await connectionRef.current.invoke("LeaveRoom", userConnection);
      } catch (e) {
        console.log(e);
      }
    }
  };

  function findChangedUser(currentUsers, newUsers, hasLeft) {
    if (currentUsers.length === 0) {
      return null;
    }

    const sourceArray = hasLeft ? currentUsers : newUsers;
    const targetArray = new Set(
      hasLeft
        ? newUsers.map((user) => user.id)
        : currentUsers.map((user) => user.id)
    );

    for (let user of sourceArray) {
      if (!targetArray.has(user.id)) {
        return user;
      }
    }

    return null;
  }

  return {
    messages,
    usersInRoom,
    hasMoreMessagesToLoad,
    setHasMoreMessagesToLoad,
    userConnection,
    leaveRoom,
    setUserConnection,
    stopConnection,
    sendMessage,
    setMessages,
  };
};
