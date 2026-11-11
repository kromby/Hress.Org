import { useEffect, useRef } from "react";
import { Link } from "react-router-dom";
import { HressPage } from "../beer/hress";
import PushBanner from "./components/PushBanner";
import {
  useMarkNotificationsSeen,
  useNotifications,
} from "./hooks/useNotifications";
import "./notifications.css";

const MONTHS = [
  "jan.",
  "feb.",
  "mar.",
  "apr.",
  "maí",
  "jún.",
  "júl.",
  "ágú.",
  "sep.",
  "okt.",
  "nóv.",
  "des.",
];

export const relativeTime = (iso: string, now: Date = new Date()): string => {
  const date = new Date(iso);
  const minutes = Math.floor((now.getTime() - date.getTime()) / 60000);
  if (minutes < 1) return "rétt í þessu";
  if (minutes < 60) return `fyrir ${minutes} mín.`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `fyrir ${hours} klst.`;
  const days = Math.floor(hours / 24);
  if (days === 1) return "í gær";
  if (days < 7) return `fyrir ${days} dögum`;
  return `${date.getDate()}. ${MONTHS[date.getMonth()]}`;
};

const NotificationsPage = () => {
  const { data, isLoading, isError } = useNotifications();
  const { mutate: markSeen } = useMarkNotificationsSeen();
  const marked = useRef(false);

  useEffect(() => {
    if (data && !marked.current) {
      marked.current = true;
      markSeen();
    }
  }, [data, markSeen]);

  const lastSeen = data?.lastSeen ?? null;

  return (
    <HressPage title="Tilkynningar">
      <PushBanner />
      {isLoading && <p>Sæki tilkynningar…</p>}
      {isError && <p>Ekki tókst að sækja tilkynningar.</p>}
      {data && data.notifications.length === 0 && (
        <p>Engar tilkynningar ennþá.</p>
      )}
      {data && data.notifications.length > 0 && (
        <ul className="notifications-list">
          {data.notifications.map((item) => {
            const unread =
              lastSeen == null || new Date(item.created) > new Date(lastSeen);
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
    </HressPage>
  );
};

export default NotificationsPage;
