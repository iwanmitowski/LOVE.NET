import TinderCard from "react-tinder-card";

import styles from "./SwipingCard.module.css";

export default function SwipingCardContainer(props) {
  const users = props.users;

  return (
    <div className={styles["cards-container"]}>
      {users.map((u) => (
        <TinderCard key={u.id} user={u} />
      ))}
    </div>
  );
}
