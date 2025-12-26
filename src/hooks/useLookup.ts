import { useMemo } from "react";
import axios, { AxiosError } from "axios";
import config from "react-global-configuration";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../context/auth";

interface Lookup {
  id: number;
  userId: number;
  typeId: number;
  valueId: number;
}

interface UseLookupResult {
  existingLookup: Lookup | undefined;
  loading: boolean;
  error: AxiosError | null;
  refetch: () => Promise<void>;
}

// Parse parentId from Href (e.g., "/api/types?parentId=226")
const getParentIdFromHref = (href: string | undefined): number | null => {
  if (!href) return null;
  const urlParams = new URLSearchParams(href.split("?")[1]);
  const parentId = urlParams.get("parentId");
  return parentId ? parseInt(parentId, 10) : null;
};

export const useLookup = (
  href: string | undefined,
  userId: number | undefined
): UseLookupResult => {
  const { authTokens } = useAuth();
  const typeId = useMemo(() => getParentIdFromHref(href), [href]);

  // Fetch existing lookup for the user
  const fetchLookup = async (): Promise<Lookup | null> => {
    if (!typeId || !userId || !authTokens?.token) {
      return null;
    }
    try {
      const lookupUrl = `${config.get(
        "apiPath"
      )}/api/users/${userId}/lookups?typeId=${typeId}`;
      const response = await axios.get<Lookup>(lookupUrl, {
        headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
      });
      return response.data;
    } catch (e) {
      // 404 is expected if no lookup exists yet
      if (axios.isAxiosError(e) && e.response?.status === 404) {
        return null;
      }
      throw e;
    }
  };

  const {
    data: existingLookup,
    isLoading: lookupLoading,
    error: lookupError,
    refetch: refetchLookup,
  } = useQuery({
    queryKey: ["lookup", userId, typeId],
    queryFn: fetchLookup,
    enabled: Boolean(typeId) && Boolean(userId) && Boolean(authTokens?.token),
    staleTime: 2 * 60 * 1000, // Consider data fresh for 2 minutes
    gcTime: 10 * 60 * 1000, // Keep unused data in cache for 10 minutes
  });

  return {
    existingLookup: existingLookup || undefined,
    loading: lookupLoading,
    error: lookupError as AxiosError | null,
    refetch: async () => {
      await refetchLookup();
    },
  };
};
