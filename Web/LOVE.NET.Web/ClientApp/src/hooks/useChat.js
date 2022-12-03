import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useState, useRef, useEffect } from "react";
import { globalConstants } from "../utils/constants";

import * as chatService from "../services/chatService";

export const useChat = () => {
  const [connection, setConnection] = useState(null);
  const [messages, setMessages] = useState([]);
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

      setConnection(newConnection);
      setUserConnection(userConnection);
      chatService.getChat(userConnection.roomId).then((res) => {
        setMessages((prevState) => [...prevState, ...res.messages]);
        setHasMoreMessagesToLoad(messages.length < res.totalMessages);
      });
    }
  }, [userConnection]);

  useEffect(() => {
    if (connection) {
      connection
        .start()
        .then(() => {
          connection.invoke("JoinRoom", userConnection);

          connection.on("ReceiveMessage", (message) => {
            const updatedMessages = [message, ...latestMessages.current];

            setMessages(updatedMessages);
          });
        })
        .catch((error) => console.log("Connection failed: ", error));
    }
  }, [connection, userConnection]);

  const sendMessage = async (message) => {
    if (connection) {
      try {
        await connection.invoke("SendMessage", message);
      } catch (e) {
        console.log(e);
      }
    }
  };

  const stopConnection = async () => {
    await connection.stop().then(() => {
      setMessages([]);
      setConnection(null);
    });
  };

  return {
    messages,
    hasMoreMessagesToLoad,
    setHasMoreMessagesToLoad,
    setUserConnection,
    stopConnection,
    sendMessage,
    setMessages,
  };
};
