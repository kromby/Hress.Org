import { QueryClient } from "@tanstack/react-query";

// Notification queries are not keyed by member, so drop them on every token change.
export const clearNotificationCache = (queryClient: QueryClient) =>
  queryClient.removeQueries({ queryKey: ["notifications"] });
