import { useState } from "react";
import { useEffect } from "react";
import SwipingCardContainer from "../../SwipingCard/SwipingCardContainer";

import * as dashboardService from "../../../services/dashboardService";

export default function UsersContainer(props) {
  const [users, setUsers] = useState([]);
  const [request, setRequest] = useState({
    showBanned: !!props.showBanned,
    search: null,
    page: 1,
  });

  useEffect(() => {
    dashboardService.getUsers(request).then((res) => {
      setUsers(res);
    });
  }, [request.showBanned, request.search]);

  return (
    <div className="container">
      <SwipingCardContainer users={users} />
    </div>
  );
}
