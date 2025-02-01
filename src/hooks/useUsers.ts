import { useMemo } from "react";
import axios, { AxiosError } from "axios";
import config from "react-global-configuration";
import { useAuth } from "../context/auth";
import { User } from "../types/legacy/user";
import { useQuery } from "@tanstack/react-query";

interface UseUsersResult {
  users: User[] | undefined;
  loading: boolean;
  error: AxiosError | null;
  refetch: () => Promise<void>;
}

export enum UserRole {
  Hardhead = "US_L_HEAD",
  All = "",
}

export const useUsers = (role: string = UserRole.All): UseUsersResult => {
  const { authTokens } = useAuth();

  const fetchUsers = async (): Promise<User[]> => {
    if (!authTokens?.token) {
      throw new Error("Authentication token is required");
    }

    if (role && !/^[a-zA-Z0-9_-]+$/.test(role)) {
      throw new Error("Role parameter must contain only letters, numbers, underscores, or hyphens");
    }

    const url = `${config.get("path")}/api/users?${role === UserRole.All ? "" : `role=${role}`}&code=${config.get("code")}`;
    const response = await axios.get<User[]>(url, {
      headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
    });
    return response.data;
  };

  const { data: unsortedUsers, isLoading, error, refetch } = useQuery({
    queryKey: ['users', role, authTokens?.token],
    queryFn: fetchUsers,
    staleTime: 5 * 60 * 1000, // Consider data fresh for 5 minutes
    gcTime: 30 * 60 * 1000, // Keep unused data in cache for 30 minutes
    enabled: !!authTokens?.token,
  });

  const users = useMemo(() => 
    unsortedUsers?.sort((a, b) => a.Name.localeCompare(b.Name)),
    [unsortedUsers]
  );

  return {
    users,
    loading: isLoading,
    error: error as AxiosError | null,
    refetch: async () => { await refetch(); }
  };
};
