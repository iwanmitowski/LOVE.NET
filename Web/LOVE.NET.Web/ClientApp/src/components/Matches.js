import { useEffect } from "react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useIdentityContext } from "../hooks/useIdentityContext";
import SwipingCardContainer from "./SwipingCard/SwipingCardContainer";

import * as datingService from "../services/datingService";
import { Fragment } from "react";
import ChatModal from "./Modals/Chat/ChatModal";

export default function Matches() {
  const navigate = useNavigate();
  const { user, isLogged, userLogout } = useIdentityContext();
  const [matches, setMatches] = useState([]);
  const [chatUser, setChatUser] = useState();

  useEffect(() => {
    if (isLogged) {
      datingService
        .getMatches(user.id)
        .then((res) => {
          setMatches(res);
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
        });
    }
  });

  return (
    <Fragment>
      <ChatModal
        show={!!chatUser}
        user={chatUser}
        onHide={() => setChatUser(null)}
      />
      <SwipingCardContainer users={matches} startChat={setChatUser} />
    </Fragment>
  );
}
