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

        if (role && !/^[a-zA-Z0-9_-]+$/.test(role)) {
          throw new Error('Role parameter must contain only letters, numbers, underscores, or hyphens');
        }

        const url = `${config.get("path")}/api/users?role=${role}&code=${config.get("code")}`;
        const response = await axios.get<User[]>(url, {
          headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
        });
        setUsers(response.data);
        setError(null);
      } catch (e) {
        if (axios.isAxiosError(e)) {
          console.error('Failed to fetch users:', e.message);
          setError(e);
        } else {
          console.error('An unexpected error occurred:', e);
          setError(new AxiosError('An unexpected error occurred'));
        }
      } finally {
        setLoading(false);
      }
    };

    if (authTokens?.token) {
      fetchUsers(role);
    }
  }, [authTokens, role]);

  return { users, loading, error };
};
