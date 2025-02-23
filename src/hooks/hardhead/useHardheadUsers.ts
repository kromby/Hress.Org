import { useState, useEffect } from "react";
import axios from "axios";
import config from "react-global-configuration";
import { HardheadUser } from "../../types/hardhead/user";

interface UseHardheadUsersResult {
  users: HardheadUser[];
  loading: boolean;
  error: string | null;
}

export const useHardheadUsersByHref = (
  href: string | undefined,
  excludeUserID?: number
): UseHardheadUsersResult => {
  const [users, setUsers] = useState<HardheadUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const getUsers = async () => {
      if (!href) {
        setUsers([]);
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);
        const url = config.get("apiPath") + href;
        const response = await axios.get<HardheadUser[]>(url);
        setUsers(
          excludeUserID
            ? response.data.filter((user) => user.ID !== excludeUserID)
            : response.data
        );
      } catch (e) {
        console.error("Failed to fetch users:", e);
        setError("Failed to fetch users");
        setUsers([]);
      } finally {
        setLoading(false);
      }
    };

    getUsers();
  }, [href, excludeUserID]);

  return { users, loading, error };
};

export const useHardheadUsers = (
  yearId: number,
  excludeUserID?: number
): UseHardheadUsersResult => {
  const href = `/api/hardhead/${yearId}/users?code=${config.get("code")}`;
  return useHardheadUsersByHref(href, excludeUserID);
};
