import { useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { Post } from "../../components";
import PushBanner from "./components/PushBanner";
import {
  useMarkNotificationsSeen,
  useNotifications,
} from "./hooks/useNotifications";
import { relativeTime } from "./relativeTime";
import { NotificationListResponse } from "./types";
import "./notifications.css";

const NotificationsPage = () => {
  const { data: cached, isError, isFetchedAfterMount } = useNotifications();
  const { mutate: markSeen } = useMarkNotificationsSeen();
  const marked = useRef(false);

  // The list from this visit's fresh load. Frozen once so the highlight
  // (based on lastSeen from before this visit) cannot change mid-view.
  const [data, setData] = useState<NotificationListResponse | null>(null);

  useEffect(() => {
    // Cached data from an earlier visit is ignored until the refetch lands.
    if (cached && isFetchedAfterMount && !marked.current) {
      marked.current = true;
      setData(cached);
      // Only mark what this visit displayed: the newest item's created value.
      const newest = cached.notifications.reduce<string | undefined>(
        (best, n) =>
          best === undefined || new Date(n.created) > new Date(best)
            ? n.created
            : best,
        undefined
      );
      markSeen(newest);
    }
  }, [cached, isFetchedAfterMount, markSeen]);

  const lastSeen = data?.lastSeen ?? null;

  return (
    <div id="main">
      <Post
        title="Tilkynningar"
        body={
          <>
            <PushBanner />
            {!data && !isError && <p>Sæki tilkynningar…</p>}
            {isError && <p>Ekki tókst að sækja tilkynningar.</p>}
            {data && data.notifications.length === 0 && (
              <p>Engar tilkynningar ennþá.</p>
            )}
            {data && data.notifications.length > 0 && (
              <ul className="notifications-list">
                {data.notifications.map((item) => {
                  const unread =
                    lastSeen === null ||
                    new Date(item.created) > new Date(lastSeen);
                  return (
                    <li key={item.id} className={unread ? "unread" : undefined}>
                      <Link to={item.link}>
                        {item.title}
                        <small>
                          {item.type === "news" ? "Ný frétt · " : ""}
                          {relativeTime(item.created)}
                        </small>
                      </Link>
                    </li>
                  );
                })}
              </ul>
            )}
          </>
        }
      />
    </div>
  );
};

export default NotificationsPage;
