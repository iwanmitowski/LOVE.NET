/* eslint-disable jsx-a11y/alt-text */
import { Button } from "react-bootstrap";
import TinderCard from "react-tinder-card";
import SwipingCardCarousel from "./SwipingCardCarousel";

import styles from "./SwipingCard.module.css";

export default function SwipingCard(props) {
  const user = props.user;

  return (
    <TinderCard
      className={styles["swipe"]}
      key={1}
      preventSwipe={["up", "down"]}
      onCardLeftScreen
      onSwipe={(dir) => console.log(dir)}
    >
      <div
        className={`${styles["card"]} ${styles["no-selecting"]}`}
        style={{ width: "30rem", margin: "0px auto" }}
      >
        <SwipingCardCarousel images={user.images} />
        <div className={styles["card-body"]}>
            <p className="card-text">
              <strong>{user.userName}</strong> {user.age}
            </p>
          <p className="card-text">{user.bio}</p>
        </div>
        <ul className="list-group list-group-flush">
          <li className="list-group-item">{user.gender}</li>
          <li className="list-group-item">{user.cityName}</li>
        </ul>
        <div className="card-body">
          <Button variant="light" type="submit">
            ❌
          </Button>
          <Button variant="light" type="submit">
            💚
          </Button>
        </div>
      </div>
    </TinderCard>
  );
}
