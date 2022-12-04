import { useState } from "react";
import { useEffect } from "react";
import SwipingCardContainer from "../../SwipingCard/SwipingCardContainer";

import * as dashboardService from "../../../services/dashboardService";

export default function UsersContainer(props) {
  const [users, setUsers] = useState([]);
  const [hasMore, setHasMore] = useState(true);
  const [request, setRequest] = useState({
    showBanned: !!props.showBanned,
    search: null,
    page: 1,
  });

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
    <SwipingCardContainer
      fetchUsers={fetchUsers}
      hasMoreUsersToLoad={hasMore}
      users={users}
    />
  );
}
