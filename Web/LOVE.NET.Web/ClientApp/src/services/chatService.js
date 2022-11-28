import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

import { globalConstants } from "../utils/constants";

export async function joinRoom(userId, roomId) {
  try {
    const connection = new HubConnectionBuilder()
      .withUrl(`${globalConstants.BASE_URL}/chat`)
      .configureLogging(LogLevel.Information)
      .build();

    connection.on("ReceiveMessage", (userId, message) => {
      console.log(message);
    });

    await connection.start();
    await connection.invoke("JoinRoom", { userId, roomId });

    return connection;
  } catch (error) {
    console.log(error);
  }
}
