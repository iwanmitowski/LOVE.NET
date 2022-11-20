import SwipingCard from "./SwipingCard";

import styles from "./SwipingCard.module.css";

export default function SwipingCardContainer(props) {
  const users = props.users;

  return (
    <div className={styles["cards-container"]}>
      {users.map((u) => (
        <SwipingCard key={u.id} user={u} />
      ))}
    </div>
  );
}
