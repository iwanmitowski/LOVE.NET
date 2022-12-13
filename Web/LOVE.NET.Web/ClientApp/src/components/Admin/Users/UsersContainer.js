import { useState, useEffect, Fragment } from "react";
import SwipingCardContainer from "../../SwipingCard/SwipingCardContainer";
import ModerateModal from "../../Modals/Moderate/ModerateModal";
import Loader from "../../Shared/Loader/Loader";
import Search from "../../Shared/Search/Search";

import * as dashboardService from "../../../services/dashboardService";

export default function UsersContainer(props) {
  const [users, setUsers] = useState([]);
  const [request, setRequest] = useState({
    showBanned: !!props.showBanned,
    hasMore: true,
    search: null,
    page: 1,
  });
  const [isLoading, setIsLoading] = useState(false);

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

  const fetchUsers = (pageReset) => {
    if (request.hasMore) {
      if (users.length === 0) {
        setIsLoading(() => true);
      }

      const page = Math.floor(users.length / 10) + 1;

      if (!!pageReset) {
        setUsers([]);
      }

      dashboardService
        .getUsers({ ...request, page: pageReset || page })
        .then((res) => {
          setUsers((prevState) => {
            let currentUsers = [...prevState, ...res.users];
            currentUsers = [
              ...new Map(
                currentUsers.map((item) => [item["id"], item])
              ).values(),
            ];

            const hasMore = currentUsers.length < res.totalUsers;
            setRequest((prevRequest) => {
              return {
                ...prevRequest,
                hasMore,
                page,
              };
            });
            return currentUsers;
          });
        })
        .finally(() => setIsLoading(() => false));
    }
  };

  useEffect(() => {
    if (users.length === 0) {
      fetchUsers();
    }
  }, []);

  return isLoading ? (
    <Loader />
  ) : (
    <Fragment>
      <Search
        search={() => fetchUsers(1)}
        setRequest={setRequest}
        request={request}
      />
      <SwipingCardContainer
        fetchUsers={fetchUsers}
        hasMoreUsersToLoad={request.hasMore}
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
