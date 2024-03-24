import { useEffect } from "react";
import { useState } from "react";
import { Link } from "react-router-dom";
import * as dashboardService from "../../../services/dashboardService";
import Loader from "../../Shared/Loader/Loader";

import StatisticCard from "./StatisticCard";

export default function Dashboard() {
  const [statistics, setStatistics] = useState();
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    setIsLoading(() => true);
    dashboardService
      .getStatistics()
      .then((res) => {
        console.log(res);
        setStatistics(res);
      })
      .finally(() => setIsLoading(() => false));
  }, []);

  return isLoading ? (
    <Loader />
  ) : (
    statistics && (
      <div className="d-flex flex-wrap justify-content-center">
        <StatisticCard
          header={
            <Link className="nav-link" to="/admin/users">
              Users
            </Link>
          }
          count={statistics.usersCount}
        />
        <StatisticCard
          header={
            <Link className="nav-link" to="/admin/users/banned">
              Banned Users
            </Link>
          }
          count={statistics.bannedUsersCount}
        />
        <StatisticCard header={"Matches"} count={statistics.matchesCount} />
        <StatisticCard header={"Likes"} count={statistics.likedUsersCount} />
        <StatisticCard
          header={"Not swiped users"}
          count={statistics.notSwipedUsersCount}
        />
        <StatisticCard header={"Images"} count={statistics.imagesCount} />
        <StatisticCard
          header={"Total messages"}
          count={statistics.messagesCount}
        />
      </div>
    )
  );
}
