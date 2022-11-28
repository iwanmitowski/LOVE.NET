import SwipingCard from "./SwipingCard";

import styles from "./SwipingCard.module.css";

export default function SwipingCardContainer(props) {
  const users = props.users;
  const swipe = props.swipe;
  const startChat = props.startChat;

  return (
    <div className={styles["cards-container"]}>
      {!!users.length ? (
        users.map((u) => (
          <SwipingCard
            key={u.id}
            user={u}
            swipe={swipe}
            startChat={startChat}
          />
        ))
      ) : (
        <h1>Come back later</h1>
      )}
    </div>
  );
}
