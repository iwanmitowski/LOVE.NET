import { useState, useEffect, Fragment } from "react";
import { useIdentityContext } from "../hooks/useIdentityContext";
import SwipingCardContainer from "./SwipingCard/SwipingCardContainer";
import { useNavigate } from "react-router-dom";
import MatchModal from "./Modals/Match/MatchModal";
import UserPreferences from "./UserPreferences/UserPreferences";
import Loader from "./Shared/Loader/Loader";
import NotLoggedHome from "./NotLoggedHome/NotLoggedHome";

import * as datingService from "../services/datingService";

export default function Home() {
  const navigate = useNavigate();
  const { isLogged, userLogout } = useIdentityContext();
  const [usersToSwipe, setUsersToSwipe] = useState([]);
  const [filteredUsers, setFilteredUsers] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [matchModel, setMatchModel] = useState({
    isMatch: false,
  });

  const swipe = (dir, swipedUserId) => {
    setUsersToSwipe((prevState) => {
      return [...prevState.filter((u) => u.id !== swipedUserId)];
    });
    setFilteredUsers((prevState) => {
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
      setIsLoading(() => true);
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
        })
        .finally(() => setIsLoading(() => false));
    }
  }, [isLogged]);

  if (!isLogged) {
    return <NotLoggedHome />;
  }

  return isLoading ? (
    <Loader isFullScreen />
  ) : (
    <Fragment>
      <UserPreferences filterUsers={setFilteredUsers} users={usersToSwipe} />
      <SwipingCardContainer users={filteredUsers} swipe={swipe} />
      <MatchModal
        show={matchModel.isMatch}
        user={matchModel.user}
        onHide={() => setMatchModel({ isMatch: false })}
      />
    </Fragment>
  );
}
