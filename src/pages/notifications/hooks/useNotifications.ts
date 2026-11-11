import { useRef } from "react";
import axios from "axios";
import config from "react-global-configuration";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useAuth } from "../../../context/auth";
import { NotificationListResponse } from "../types";

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

export const useMarkNotificationsSeen = () => {
  const { authTokens } = useAuth();
  const queryClient = useQueryClient();
  const token: string | undefined = authTokens?.token;
  // Always the member signed in right now, even from a stale mutation callback.
  const currentToken = useRef(token);
  currentToken.current = token;

  return useMutation<void, unknown, string | undefined, string | undefined>({
    // Remember who started the request.
    onMutate: () => token,
    mutationFn: async (upTo) => {
      await axios.post(
        `${config.get("apiPath")}/api/notifications/seen`,
        upTo === undefined ? undefined : { upTo },
        { headers: { "X-Custom-Authorization": `token ${token}` } }
      );
    },
    onSuccess: async (_data, _upTo, startedWith) => {
      // The token changed while the request was pending: another member's
      // status must not be overwritten.
      if (startedWith !== currentToken.current) {
        return;
      }
      // A status refetch still in flight may carry pre-seen data; drop it first.
      await queryClient.cancelQueries({
        queryKey: ["notifications", "status"],
      });
      if (startedWith !== currentToken.current) {
        return;
      }
      queryClient.setQueryData(["notifications", "status"], {
        hasUnread: false,
      });
    },
  });
};
