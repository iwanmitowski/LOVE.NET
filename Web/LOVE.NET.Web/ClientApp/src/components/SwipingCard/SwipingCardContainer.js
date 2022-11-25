import SwipingCard from "./SwipingCard";

import styles from "./SwipingCard.module.css";

export default function SwipingCardContainer(props) {
  const users = props.users;
  const swipe = props.swipe;

  return (
    <div className={styles["cards-container"]}>
      {!!users.length ? (
        users.map((u) => <SwipingCard key={u.id} user={u} swipe={swipe} />)
      ) : (
        <h1>Come back later</h1>
      )}
    </div>
  );
}
