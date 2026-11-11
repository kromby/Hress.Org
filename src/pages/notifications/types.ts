export type NotificationItem = {
  id: string;
  type: string;
  title: string;
  link: string;
  created: string;
};

export type NotificationListResponse = {
  lastSeen: string | null;
  notifications: NotificationItem[];
};

export type PushState =
  "unsupported" | "ios-install" | "blocked" | "off" | "on" | "error";
