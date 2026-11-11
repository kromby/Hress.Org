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
    enabled: !!authTokens?.token,
  });
};

export const useMarkNotificationsSeen = () => {
  const { authTokens } = useAuth();
  const queryClient = useQueryClient();

  return useMutation<void, unknown, void>({
    mutationFn: async () => {
      await axios.post(
        `${config.get("apiPath")}/api/notifications/seen`,
        undefined,
        { headers: { "X-Custom-Authorization": `token ${authTokens?.token}` } }
      );
    },
    onSuccess: () => {
      queryClient.setQueryData(["notifications", "status"], {
        hasUnread: false,
      });
    },
  });
};
