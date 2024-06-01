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
  const showPreferences = props.showPreferences;

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
        <div
          className="container"
          style={{
            position: "absolute",
            left: showPreferences ? "0" : "12%",
            top: "40%",
            right: "0",
            marginLeft: "auto",
            marginRight: "auto",
            width: "480px",
          }}
        >
          <h1 className="text-center">Come back later</h1>
        </div>
      )}
    </InfiniteScroll>
  );
}
