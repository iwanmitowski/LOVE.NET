import InfiniteScroll from "react-infinite-scroll-component";
import SwipingCard from "./SwipingCard";

import styles from "./SwipingCard.module.css";

export default function SwipingCardContainer(props) {
  const users = props.users;
  const swipe = props.swipe;
  const startChat = props.startChat;

  const fetchUsers = props.fetchUsers;
  const hasMoreUsersToLoad = props.hasMoreUsersToLoad;

  return (
    <InfiniteScroll
      dataLength={users.length}
      next={fetchUsers}
      hasMore={hasMoreUsersToLoad}
      style={{ display: "flex", flexWrap: "wrap" }}
      scrollableTarget="scrollableDiv"
    >
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
        <div className="container">
          <h1 className="text-center">Come back later</h1>
        </div>
      )}
    </InfiniteScroll>
  );
}
