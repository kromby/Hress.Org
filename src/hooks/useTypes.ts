import axios, { AxiosError } from "axios";
import config from "react-global-configuration";
import { useQuery } from "@tanstack/react-query";
import { TypeEntity } from "../types/typeEntity";
import { useAuth } from "../context/auth";

interface UseTypesResult {
  types: TypeEntity[] | undefined;
  loading: boolean;
  error: AxiosError | null;
  refetch: () => Promise<void>;
}

export const useTypes = (href: string | undefined): UseTypesResult => {
  const { authTokens } = useAuth();

  // Fetch types from the Href
  const fetchTypes = async (): Promise<TypeEntity[]> => {
    if (!href || !authTokens?.token) {
      return [];
    }
    const typesUrl = `${config.get("apiPath")}${href}`;
    const response = await axios.get<TypeEntity[]>(typesUrl, {
      headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
    });
    return response.data || [];
  };

  const {
    data: types,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: ["types", href],
    queryFn: fetchTypes,
    enabled: Boolean(href) && Boolean(authTokens?.token),
    staleTime: 5 * 60 * 1000, // Consider data fresh for 5 minutes
    gcTime: 30 * 60 * 1000, // Keep unused data in cache for 30 minutes
  });

  return {
    types,
    loading: isLoading,
    error: error as AxiosError | null,
    refetch: async () => {
      await refetch();
    },
  };
};
