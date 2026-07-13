// SignalR end-to-end probe, executed inside the frontend container (docker
// exec) so it can reuse the app's @microsoft/signalr and ws packages and
// reach the gateway over the compose network.
//
// Env: GATEWAY_URL (default http://gateway:8080), TOKEN, ROOM_ID, MESSAGE_TEXT.
// Prints HUB_OK <json> and exits 0 when the sent message echoes back.

const signalR = require("@microsoft/signalr");
const WebSocket = require("ws");

const gateway = process.env.GATEWAY_URL || "http://gateway:8080";
const token = process.env.TOKEN;
const roomId = process.env.ROOM_ID;
const text = process.env.MESSAGE_TEXT || "smoke-test message";

(async () => {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${gateway}/chat`, {
      accessTokenFactory: () => token,
      // Node has no browser fetch/WebSocket: skip the negotiate HTTP call and
      // go straight to websockets with the ws package.
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
      WebSocket,
    })
    .build();

  let received = null;
  connection.on("ReceiveMessage", (message) => {
    received = message;
  });

  await connection.start();
  await connection.invoke("JoinRoom", {
    roomId,
    userId: null,
    userName: null,
    profilePictureUrl: null,
  });
  await connection.invoke("SendMessage", {
    roomId,
    text,
    imageUrl: null,
    profilePicture: null,
  });

  const deadline = Date.now() + 15000;
  while (!received && Date.now() < deadline) {
    await new Promise((resolve) => setTimeout(resolve, 200));
  }

  if (!received || received.text !== text) {
    console.error("FAIL: ReceiveMessage did not echo the sent message", received);
    process.exit(1);
  }

  console.log("HUB_OK " + JSON.stringify(received));
  await connection.stop();
  process.exit(0);
})().catch((error) => {
  console.error("FAIL:", error.message);
  process.exit(1);
});
