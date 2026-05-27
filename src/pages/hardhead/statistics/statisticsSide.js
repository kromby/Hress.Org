import { useState, useEffect } from "react";
import config from "react-global-configuration";
import { MiniPost } from "../../../components";
import axios from "axios";

const StatisticsSide = () => {
  const [data, setData] = useState({
    stats: null,
    isLoading: false,
    visible: false,
  });

  useEffect(() => {
    const getAwards = async () => {
      try {
        const isHost = Math.random() < 0.5;
        const periodType = Math.round(Math.random() * 4);
        const params = new URLSearchParams({ periodType });
        if (isHost) params.set("attendanceType", "53");
        const url = `${config.get("apiPath")}/api/hardhead/statistics/users?${params}`;

        setData({ isLoading: true });
        const response = await axios.get(url);
        setData({ stats: response.data, isLoading: false, visible: true });
      } catch (e) {
        console.error(e);
        setData({ isLoading: false, visible: false });
      }
    };

    if (!data.stats) {
      getAwards();
    }
  }, []);

  const getDescription = (period, guest) => {
    let description = "gestur";

    if (guest === "gestur") description = "Oftast mætt";
    else description = "Oftast haldið";

    if (period === "All") description = `${description} frá upphafi`;
    else if (period === "Last10")
      description = `${description} síðustu 10 álin`;
    else if (period === "Last5") description = `${description} síðustu 5 álin`;
    else if (period === "Last2") description = `${description} síðustu 2 álin`;
    else if (period === "ThisYear") description = `${description}  á þessu ári`;

    return description;
  };

  const top = data.stats ? data.stats.list[0] : null;

  return (
    <div>
      {data.visible && top ? (
        <MiniPost
          title="Tölfræði"
          href="/hardhead/stats"
          description={
            <span>
              {getDescription(data.stats.periodTypeName, data.stats.typeName)}
              <br />
              {top.user.username} - {top.attendedCount}
              <br />
              {top.firstAttendedString} - {top.lastAttendedString}
            </span>
          }
          date={data.stats.dateFrom}
          dateString={data.stats.dateFromString}
          userHref={`/hardhead/users/${top.user.id}`}
          userPhoto={
            top.user.profilePhoto
              ? config.get("apiPath") + top.user.profilePhoto.href
              : undefined
          }
          userText={top.user.username}
        />
      ) : null}
    </div>
  );
};

export default StatisticsSide;
