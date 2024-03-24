import InfiniteScroll from "react-infinite-scroll-component";
import Loader from "../Shared/Loader/Loader";
import SwipingCard from "./SwipingCard";

export default function SwipingCardContainer(props) {
  const users = props.users;
  const swipe = props.swipe;
  const startChat = props.startChat;

  const fetchUsers = props.fetchUsers;
  const hasMoreUsersToLoad = props.hasMoreUsersToLoad;
  const setUserBanRequest = props.setUserBanRequest;

  return (
    <InfiniteScroll
      dataLength={users.length}
      next={fetchUsers}
      hasMore={hasMoreUsersToLoad}
      style={{ display: "flex", flexWrap: "wrap" }}
      scrollableTarget="scrollableDiv"
      loader={<Loader />}
    >
      {!!users.length ? (
        users.map((u) => (
          <SwipingCard
            key={u.id}
            user={u}
            swipe={swipe}
            startChat={startChat}
            setUserBanRequest={setUserBanRequest}
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
