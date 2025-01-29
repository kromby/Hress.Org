import { useState, useEffect } from "react";
import axios, { AxiosError } from "axios";
import config from "react-global-configuration";
import { useAuth } from "../context/auth";
import { User } from "../types/user";

interface UseUsersResult {
  users: User[] | undefined;
  loading: boolean;
  error: AxiosError | null;
}

export const useUsers = (role: string): UseUsersResult => {
  const [users, setUsers] = useState<User[]>();
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<AxiosError | null>(null);
  const { authTokens } = useAuth();

  useEffect(() => {
    const fetchUsers = async (role: string): Promise<void> => {
      try {
        setLoading(true);
        const url = `${config.get("path")}/api/users?role=${role}&code=${config.get("code")}`;
        const response = await axios.get<User[]>(url, {
          headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
        });
        setUsers(response.data);
        setError(null);
      } catch (e) {
        console.error(e);
        setError(e as AxiosError);
      } finally {
        setLoading(false);
      }
    };

    if (authTokens?.token) {
      fetchUsers(role);
    }
  }, [authTokens]);

  return { users, loading, error };
};
