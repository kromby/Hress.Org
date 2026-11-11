import axios from "axios";
import config from "react-global-configuration";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../../../context/auth";

export const useNotificationStatus = () => {
  const { authTokens } = useAuth();

  return useQuery({
    queryKey: ["notifications", "status"],
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
