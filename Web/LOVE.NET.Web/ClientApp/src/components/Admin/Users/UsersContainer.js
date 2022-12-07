import { useState, useEffect, Fragment } from "react";
import SwipingCardContainer from "../../SwipingCard/SwipingCardContainer";
import ModerateModal from "../../Modals/Moderate/ModerateModal";

import * as dashboardService from "../../../services/dashboardService";

export default function UsersContainer(props) {
  const [users, setUsers] = useState([]);
  const [hasMore, setHasMore] = useState(true);
  const [request, setRequest] = useState({
    showBanned: !!props.showBanned,
    search: null,
    page: 1,
  });

  const defaultUserBanRequest = {
    user: null,
    userId: "",
    bannedUntil: new Date().toISOString().split("T")[0],
  };

  const [userBanRequest, setUserBanRequest] = useState(defaultUserBanRequest);
  const onUserModerateClose = (updatedUser) => {
    setUserBanRequest(defaultUserBanRequest);
    if (!!updatedUser) {
      setUsers((prevState) => {
        const updatedUsers = prevState.map((u) =>
          u.id === updatedUser.id ? { ...u, isBanned: updatedUser.isBan } : u
        );
        return updatedUsers;
      });
    }
  };

  const fetchUsers = () => {
    if (hasMore) {
      const page = Math.floor(users.length / 10) + 1;

      dashboardService.getUsers({ ...request, page }).then((res) => {
        setUsers((prevState) => {
          const currentUsers = [...prevState, ...res.users];
          const hasMore = currentUsers.length < res.totalUsers;
          setHasMore(hasMore);
          setRequest((prevRequest) => {
            return {
              ...prevRequest,
              page,
            };
          });
          return currentUsers;
        });
      });
    }
  };

  useEffect(() => {
    if (users.length === 0) {
      fetchUsers();
    }
  }, []);

  return (
    <Fragment>
      <SwipingCardContainer
        fetchUsers={fetchUsers}
        hasMoreUsersToLoad={hasMore}
        users={users}
        setUserBanRequest={setUserBanRequest}
      />
      <ModerateModal
        onHide={onUserModerateClose}
        show={!!userBanRequest.user}
        userBanRequest={userBanRequest}
        setUserBanRequest={setUserBanRequest}
      />
    </Fragment>
  );
}
