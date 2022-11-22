/* eslint-disable jsx-a11y/alt-text */
import { Button } from "react-bootstrap";
import TinderCard from "react-tinder-card";
import SwipingCardCarousel from "./SwipingCardCarousel";

import styles from "./SwipingCard.module.css";

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
  const user = props.user;
  const swipe = props.swipe;

  const leftMemoizedCallback = debounce(() => swipe("left", user.id));
  const rightMemoizedCallback = debounce(() => swipe("right", user.id));

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
      onCardLeftScreen={()=> console.log(123654987)}
    >
      <div
        className={`${styles["card"]} ${styles["no-selecting"]}`}
        style={{ width: "30rem", margin: "0px auto" }}
      >
        <SwipingCardCarousel images={user.images} />
        <div className={`m-3 ${styles["card-body"]}`}>
          <p className={`card-text ${styles.userName}`}>
            <strong>{user.userName}</strong> {user.age}
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
            onClick={() => leftMemoizedCallback()}
          >
            ❌
          </Button>
          <Button
            variant="light"
            type="submit"
            onClick={() => {
              rightMemoizedCallback();
            }}
          >
            💚
          </Button>
        </div>
      </div>
    </TinderCard>
  );
}
