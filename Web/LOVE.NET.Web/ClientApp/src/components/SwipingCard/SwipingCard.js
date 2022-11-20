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
        <SwipingCardCarousel
          images={[
            "https://i.picsum.photos/id/1011/5472/3648.jpg?hmac=Koo9845x2akkVzVFX3xxAc9BCkeGYA9VRVfLE4f0Zzk",
            "https://res.cloudinary.com/dojl8gfnd/image/upload/v1668613576/zhvlfmiporzpr1zq3bjg.png",
          ]}
        />
        <div className="card-body">
          <div className="d-inline-block">
            <h4 className="card-title">
              <strong>user.userName</strong> user.age
            </h4>
          </div>
          <p className="card-text">
            Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean
            commodo ligula eget dolor. Aenean massa. Cum sociis natoque
            penatibus et magnis dis parturient montes, nascetur ridiculus mus.
            Donec quam felis, ultricies nec, pellentesque eu, pretium.12345
          </p>
        </div>
        <ul className="list-group list-group-flush">
          <li className="list-group-item">user.gender</li>
          <li className="list-group-item">user.cityName</li>
          <li className="list-group-item">Vestibulum at eros</li>
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
