/* eslint-disable jsx-a11y/alt-text */
import { useCallback } from "react";
import { Button } from "react-bootstrap";
import TinderCard from "react-tinder-card";
import SwipingCardCarousel from "./SwipingCardCarousel";

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
  const { location } = useIdentityContext();

  const user = props.user;
  const swipe = props.swipe;

  const currentDistance = distance.inKms(
    location.latitude,
    location.longitude,
    user.latitude,
    user.longitude
  );

  return (
    <TinderCard
      className={styles["swipe"]}
      key={user.id}
      preventSwipe={["up", "down"]}
      onSwipe={(dir) => {
        setTimeout(() => {
          debounce(() => swipe(dir, user.id))();
        }, 1000);
      }}
    >
      <div
        className={`${styles["card"]} ${styles["no-selecting"]}`}
        style={{ width: "30rem", margin: "0px auto" }}
      >
        <SwipingCardCarousel images={user.images} />
        <div className={`m-3 ${styles["card-body"]}`}>
          <p className={`card-text ${styles.userName}`}>
            <strong>{user.userName}</strong> {user.age} - {currentDistance} kms
          </p>
          <p className={`card-text ${styles.bio}`}>{user.bio}</p>
        </div>
        <ul className="list-group list-group-flush">
          <li className="list-group-item">{user.genderName}</li>
          <li className="list-group-item">{user.cityName}</li>
        </ul>
        <div className="card-body">
          <Button
            variant="light"
            type="submit"
            onClick={() => swipe("left", user.id)}
          >
            ❌
          </Button>
          <Button
            variant="light"
            type="submit"
            onClick={() => swipe("right", user.id)}
          >
            💚
          </Button>
        </div>
      </div>
    </TinderCard>
  );
}
