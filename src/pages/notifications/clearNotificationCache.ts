import { QueryClient } from "@tanstack/react-query";

// The list query is not keyed by member, so drop all notification queries on every token change.
export const clearNotificationCache = (queryClient: QueryClient) =>
  queryClient.removeQueries({ queryKey: ["notifications"] });
