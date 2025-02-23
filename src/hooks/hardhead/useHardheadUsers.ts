import { useState, useEffect } from "react";
import axios from "axios";
import config from "react-global-configuration";
import { HardheadUser } from "../../types/hardhead/user";

interface UseHardheadUsersProps {
  excludeUserID?: string;
}

export const useHardheadUsers = ({
  excludeUserID,
}: UseHardheadUsersProps = {}) => {
  const [users, setUsers] = useState<HardheadUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const YEAR_ID = process.env.REACT_APP_NOMINATIONS_YEAR_ID || "5437";
  const url = `${config.get(
    "path"
  )}/api/hardhead/${YEAR_ID}/users?code=${config.get("code")}`;

  useEffect(() => {
    const getUsers = async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await axios.get<HardheadUser[]>(url);
        setUsers(
          excludeUserID
            ? response.data.filter(
                (user) => user.ID.toString() !== excludeUserID
              )
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
  }, [url, excludeUserID]);

  return { users, loading, error };
};
