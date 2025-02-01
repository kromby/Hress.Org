import { useMemo } from "react";
import axios, { AxiosError } from "axios";
import config from "react-global-configuration";
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

  const fetchUsers = async (): Promise<User[]> => {
    const url = `${config.get("path")}/api/users?${role === UserRole.All ? "" : `role=${role}`}&code=${config.get("code")}`;
    const response = await axios.get<User[]>(url);
    return response.data;
  };

  const { data: unsortedUsers, isLoading, error, refetch } = useQuery({
    queryKey: ['users', role],
    queryFn: fetchUsers,
    staleTime: 5 * 60 * 1000, // Consider data fresh for 5 minutes
    gcTime: 30 * 60 * 1000, // Keep unused data in cache for 30 minutes
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
