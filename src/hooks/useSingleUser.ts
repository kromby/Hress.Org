import { useState, useEffect, useCallback } from "react";
import axios, { AxiosError } from "axios";
import config from "react-global-configuration";
import { UserBasicEntity } from "../types/userBasicEntity";
import { userCache } from "./useUserCache";

interface UseSingleUserResult {
  user: UserBasicEntity | undefined;
  loading: boolean;
  error: AxiosError | null;
  refetch: () => Promise<void>;
}

export const useSingleUser = (href: string | undefined): UseSingleUserResult => {
  const [user, setUser] = useState<UserBasicEntity>();
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<AxiosError | null>(null);

  const fetchUser = useCallback(async (bypassCache = false): Promise<void> => {
    if (!href) return;

    // Check cache first unless bypassing
    if (!bypassCache) {
      const cachedUser = userCache.get(href);
      if (cachedUser) {
        setUser(cachedUser);
        setError(null);
        return;
      }
    }

    try {
      setLoading(true);
      const url = config.get("apiPath") + href;
      const response = await axios.get<UserBasicEntity>(url);
      const fetchedUser = response.data;
      
      // Update cache with new data
      userCache.set(href, fetchedUser);
      
      setUser(fetchedUser);
      setError(null);
    } catch (e) {
      if (axios.isAxiosError(e)) {
        console.error("Failed to fetch user:", e.message);
        setError(e);
      } else {
        console.error("An unexpected error occurred:", e);
        setError(new AxiosError("An unexpected error occurred"));
      }
    } finally {
      setLoading(false);
    }
  }, [href]);

  // Memoize the refetch function to avoid unnecessary re-renders
  const refetch = useCallback(() => fetchUser(true), [fetchUser]);

  useEffect(() => {
    if (href) {
      fetchUser(false);
    } else {
      setUser(undefined);
      setError(null);
      setLoading(false);
    }
  }, [href, fetchUser]);

  return { 
    user, 
    loading, 
    error, 
    refetch
  };
};
