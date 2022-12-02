import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useState, useRef, useEffect } from "react";
import { globalConstants } from "../utils/constants";

import * as chatService from "../services/chatService";

export const useChat = () => {
  const [connection, setConnection] = useState(null);
  const [messages, setMessages] = useState([]);
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
      chatService
        .getChat(userConnection.roomId)
        .then((res) => setMessages((prevState) => [...prevState, ...res]));
    }
  }, [userConnection]);

  useEffect(() => {
    if (connection) {
      connection
        .start()
        .then(() => {
          connection.invoke("JoinRoom", userConnection);

          connection.on("ReceiveMessage", (message) => {
            const updatedMessages = [...latestMessages.current];
            updatedMessages.push(message);

            setMessages(updatedMessages);
          });
        })
        .catch((error) => console.log("Connection failed: ", error));
      console.log("messages");
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
    connection.stop().then(() => {
      setMessages([]);
      setConnection(null);
    });
  };

  return [messages, setUserConnection, stopConnection, sendMessage];
};
