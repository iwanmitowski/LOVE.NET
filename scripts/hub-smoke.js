// SignalR end-to-end probe, executed inside the frontend container (docker
// exec) so it can reuse the app's @microsoft/signalr and ws packages and
// reach the gateway over the compose network.
//
// Env: GATEWAY_URL (default http://gateway:8080), TOKEN, ROOM_ID, MESSAGE_TEXT,
// and optionally TOKEN_B (the peer): when set, a second connection joins as
// the peer, waits for the typing signal + message, and marks the room read.
// Prints HUB_OK <json> (and PEER_OK <json> with TOKEN_B) and exits 0.

const signalR = require("@microsoft/signalr");
const WebSocket = require("ws");

const gateway = process.env.GATEWAY_URL || "http://gateway:8080";
const token = process.env.TOKEN;
const tokenB = process.env.TOKEN_B;
const roomId = process.env.ROOM_ID;
const text = process.env.MESSAGE_TEXT || "smoke-test message";

function connect(jwt) {
  return new signalR.HubConnectionBuilder()
    .withUrl(`${gateway}/chat`, {
      accessTokenFactory: () => jwt,
      // Node has no browser fetch/WebSocket: skip the negotiate HTTP call and
      // go straight to websockets with the ws package.
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
      WebSocket,
    })
    .build();
}

async function waitFor(predicate, timeoutMs) {
  const deadline = Date.now() + timeoutMs;
  while (!predicate() && Date.now() < deadline) {
    await new Promise((resolve) => setTimeout(resolve, 200));
  }
  return predicate();
}

(async () => {
  const sender = connect(token);

  let received = null;
  sender.on("ReceiveMessage", (message) => {
    received = message;
  });

  let peerRead = null;
  sender.on("ReadReceipt", (receipt) => {
    peerRead = receipt;
  });

  await sender.start();
  await sender.invoke("JoinRoom", { roomId, profilePictureUrl: null });

  // Peer connection (userB): sees typing + message, then marks the room read.
  let peer = null;
  let peerGotTyping = null;
  let peerGotMessage = null;

  if (tokenB) {
    peer = connect(tokenB);
    peer.on("PeerTyping", (typing) => {
      peerGotTyping = typing;
    });
    peer.on("ReceiveMessage", (message) => {
      peerGotMessage = message;
    });
    await peer.start();
    await peer.invoke("JoinRoom", { roomId, profilePictureUrl: null });
    await sender.invoke("Typing", roomId);
  }

  await sender.invoke("SendMessage", {
    roomId,
    text,
    imageUrl: null,
    profilePicture: null,
  });

  if (!(await waitFor(() => received, 15000)) || received.text !== text) {
    console.error("FAIL: ReceiveMessage did not echo the sent message", received);
    process.exit(1);
  }

  if (!received.id) {
    console.error("FAIL: ReceiveMessage payload has no server message id", received);
    process.exit(1);
  }

  console.log("HUB_OK " + JSON.stringify(received));

  if (peer) {
    if (!(await waitFor(() => peerGotTyping && peerGotMessage, 15000))) {
      console.error("FAIL: peer missed typing/message", peerGotTyping, peerGotMessage);
      process.exit(1);
    }

    await peer.invoke("MarkRead", roomId);

    // The sender should see the peer's read receipt come back live.
    if (!(await waitFor(() => peerRead, 15000))) {
      console.error("FAIL: sender never received ReadReceipt");
      process.exit(1);
    }

    console.log("PEER_OK " + JSON.stringify({ typing: !!peerGotTyping, readAt: peerRead.readAt }));
    await peer.stop();
  }

  await sender.stop();
  process.exit(0);
})().catch((error) => {
  console.error("FAIL:", error.message);
  process.exit(1);
});
