import { useEffect, useRef } from "react";
import { Link, useLocation } from "react-router-dom";
import { useAuth } from "../../context/auth";
import { useNotificationStatus } from "../notifications/hooks/useNotificationStatus";
import "./notificationBell.css";

const NotificationBell = () => {
  const { authTokens } = useAuth();
  const { pathname } = useLocation();
  const { data, refetch } = useNotificationStatus();
  const lastPathname = useRef(pathname);

  // Re-check for unread notifications whenever the user navigates.
  useEffect(() => {
    if (lastPathname.current !== pathname) {
      lastPathname.current = pathname;
      if (authTokens?.token) {
        refetch();
      }
    }
  }, [pathname, authTokens, refetch]);

  if (!authTokens) {
    return null;
  }

  return (
    <li className="notifications">
      <Link className="fa-bell" to="/notifications">
        Tilkynningar
      </Link>
      {data?.hasUnread && (
        <>
          <span className="notification-dot" aria-hidden="true" />
          <span className="visually-hidden">Ólesnar tilkynningar</span>
        </>
      )}
    </li>
  );
};

export default NotificationBell;
