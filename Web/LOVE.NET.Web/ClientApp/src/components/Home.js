import { useState, useEffect, Fragment } from "react";
import { useIdentityContext } from "../hooks/useIdentityContext";
import SwipingCardContainer from "./SwipingCard/SwipingCardContainer";

import * as datingService from "../services/datingService";
import { useNavigate } from "react-router-dom";
import { Modal } from "react-bootstrap";
import MatchModal from "./Modal/MatchModal";

export default function Home() {
  const navigate = useNavigate();
  const { isLogged, userLogout } = useIdentityContext();
  const [usersToSwipe, setUsersToSwipe] = useState([]);
  const [matchModel, setMatchModel] = useState({
    isMatch: false,
  });

  const swipe = (dir, swipedUserId) => {
    setUsersToSwipe((prevState) => {
      return [...prevState.filter((u) => u.id !== swipedUserId)];
    });
    if (dir === "right") {
      datingService
        .likeUser(swipedUserId)
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
    }
  };

  useEffect(() => {
    if (isLogged) {
      datingService
        .getUsersToSwipe()
        .then((res) => {
          setUsersToSwipe(res);
        })
        .catch((error) => {
          if (
            error?.response?.status === 401 ||
            error?.message?.includes("status code 401")
          ) {
            userLogout();
          } else if (
            error?.response?.status === 404 ||
            error?.message?.includes("status code 404")
          ) {
            navigate("/notfound");
          }
        });
    }
  }, [isLogged]);

  if (!isLogged) {
    return <h1>Don't you want to find your beloved one ?</h1>;
  }
  console.log(usersToSwipe?.length);
  return (
    <Fragment>
      <h1>Home</h1>
      {!!usersToSwipe?.length ? (
        <SwipingCardContainer users={usersToSwipe} swipe={swipe} />
      ) : (
        <h1>Come back later</h1>
      )}
      <MatchModal
        show={matchModel.isMatch}
        user={matchModel.user}
        onHide={() => setMatchModel({ isMatch: false })}
      />
    </Fragment>
  );
}
