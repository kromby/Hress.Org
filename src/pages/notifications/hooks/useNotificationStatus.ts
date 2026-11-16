import axios from "axios";
import config from "react-global-configuration";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../../../context/auth";

// Keyed by member, so a late write for one member can never land on another's status.
export const notificationStatusKey = (token: string | undefined) =>
  ["notifications", "status", token ?? ""] as const;

export const useNotificationStatus = () => {
  const { authTokens } = useAuth();

  return useQuery({
    queryKey: notificationStatusKey(authTokens?.token),
    queryFn: async (): Promise<{ hasUnread: boolean }> => {
      const response = await axios.get<{ hasUnread?: boolean }>(
        `${config.get("apiPath")}/api/notifications/status`,
        { headers: { "X-Custom-Authorization": `token ${authTokens?.token}` } }
      );
      // The API omits false values, so a missing flag means false.
      return { hasUnread: response.data?.hasUnread === true };
    },
    enabled: Boolean(authTokens?.token),
    retry: false,
  });
};
