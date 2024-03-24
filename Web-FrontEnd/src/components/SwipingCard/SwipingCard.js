/* eslint-disable jsx-a11y/alt-text */
import { Fragment, useCallback } from "react";
import { Button } from "react-bootstrap";
import TinderCard from "react-tinder-card";
import SwipingCardCarousel from "./SwipingCardCarousel";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCommentAlt } from "@fortawesome/free-solid-svg-icons";

import * as distance from "../../utils/distance";

import styles from "./SwipingCard.module.css";
import { useIdentityContext } from "../../hooks/useIdentityContext";

// Debouncer for safety for delaying requests to the server
function debounce(func, timeout = 3000) {
  let timer;
  return (...args) => {
    clearTimeout(timer);
    timer = setTimeout(() => {
      func.apply(this, args);
    }, timeout);
  };
}

export default function SwipingCard(props) {
  const { user, location } = useIdentityContext();

  const currentUser = props.user;
  const swipe = props.swipe;
  const startChat = props.startChat;
  const setUserBanRequest = props.setUserBanRequest;

  const currentDistance = distance.inKms(
    location.latitude,
    location.longitude,
    currentUser.latitude,
    currentUser.longitude
  );

  const cardContent = (
    <div
      className={`${styles["card"]} ${styles["no-selecting"]}`}
      style={{ width: "30rem", margin: "30px auto" }}
    >
      <SwipingCardCarousel images={currentUser.images} />
      <div className={`m-3 ${styles["card-body"]}`}>
        <p className={`card-text ${styles.userName}`}>
          <strong>{currentUser.userName}</strong> {currentUser.age} -{" "}
          {currentDistance} kms
        </p>
        <p className={`card-text ${styles.bio}`}>{currentUser.bio}</p>
      </div>
      <ul className="list-group list-group-flush">
        <li className="list-group-item">{currentUser.genderName}</li>
        <li className="list-group-item">{currentUser.cityName}</li>
      </ul>
      <div className="card-body">
        {!!swipe && (
          <Fragment>
            <Button
              className="m-3"
              variant="light"
              type="submit"
              onClick={() => swipe("left", currentUser.id)}
            >
              ❌
            </Button>
            <Button
              className="m-3"
              variant="light"
              type="submit"
              onClick={() => swipe("right", currentUser.id)}
            >
              💚
            </Button>
          </Fragment>
        )}
        {user.isAdmin && (
          <Fragment>
            {!currentUser.isAdmin && (
              <Fragment>
                {currentUser.isBanned ? (
                  <Button
                    className="m-2"
                    onClick={() =>
                      setUserBanRequest((prevState) => {
                        return {
                          ...prevState,
                          user: currentUser,
                          userId: currentUser.id,
                          isBan: false,
                        };
                      })
                    }
                  >
                    {" "}
                    Unban{" "}
                  </Button>
                ) : (
                  <Button
                    className="m-2"
                    variant="danger"
                    onClick={() =>
                      setUserBanRequest((prevState) => {
                        return {
                          ...prevState,
                          user: currentUser,
                          userId: currentUser.id,
                          isBan: true,
                        };
                      })
                    }
                  >
                    {" "}
                    Ban{" "}
                  </Button>
                )}
              </Fragment>
            )}
          </Fragment>
        )}
        {!!startChat && (
          <Button
            variant="light"
            type="submit"
            onClick={() => startChat(currentUser)}
          >
            <FontAwesomeIcon icon={faCommentAlt} />
          </Button>
        )}
      </div>
    </div>
  );

  if (!swipe) {
    return cardContent;
  }

  return (
    <TinderCard
      className={styles["swipe"]}
      key={currentUser.id}
      preventSwipe={["up", "down"]}
      onSwipe={(dir) => {
        setTimeout(() => {
          debounce(() => swipe(dir, currentUser.id))();
        }, 1000);
      }}
    >
      {cardContent}
    </TinderCard>
  );
}
