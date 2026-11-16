import axios from "axios";
import config from "react-global-configuration";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useAuth } from "../../../context/auth";
import { NotificationListResponse } from "../types";
import { notificationStatusKey } from "./useNotificationStatus";

export const useNotifications = () => {
  const { authTokens } = useAuth();

  return useQuery({
    queryKey: ["notifications"],
    queryFn: async (): Promise<NotificationListResponse> => {
      const response = await axios.get<Partial<NotificationListResponse>>(
        `${config.get("apiPath")}/api/notifications`,
        { headers: { "X-Custom-Authorization": `token ${authTokens?.token}` } }
      );
      // The API omits null values, so normalize to the typed shape.
      return {
        lastSeen: response.data?.lastSeen ?? null,
        notifications: response.data?.notifications ?? [],
      };
    },
    enabled: Boolean(authTokens?.token),
  });
};

// upTo: the created time of the newest notification shown; omit to mark all.
export type SeenVariables = { upTo?: string };

export const useMarkNotificationsSeen = () => {
  const { authTokens } = useAuth();
  const queryClient = useQueryClient();
  const token: string | undefined = authTokens?.token;

  return useMutation({
    // Remember who started the request; the callbacks below may run after
    // that member signed out and the page unmounted.
    onMutate: (): string | undefined => token,
    mutationFn: async ({ upTo }: SeenVariables) => {
      await axios.post(
        `${config.get("apiPath")}/api/notifications/seen`,
        upTo === undefined ? undefined : { upTo },
        { headers: { "X-Custom-Authorization": `token ${token}` } }
      );
    },
    onSuccess: async (_data, _variables, startedWith) => {
      const key = notificationStatusKey(startedWith);
      // Signed out meanwhile: that member's status is gone from the cache.
      if (!queryClient.getQueryState(key)) {
        return;
      }
      // A status refetch still in flight may carry pre-seen data; drop it,
      // clear the dot right away, then ask the server, which still reports
      // anything newer than what was shown.
      await queryClient.cancelQueries({ queryKey: key });
      queryClient.setQueryData(key, { hasUnread: false });
      await queryClient.invalidateQueries({ queryKey: key });
    },
  });
};
