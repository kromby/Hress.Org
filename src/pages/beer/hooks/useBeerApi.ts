import { useQuery } from "@tanstack/react-query";
import axios from "axios";
import { hressApiUrl, useHressAuth } from "../hress";
import { Season } from "../types";

// The backend omits default and null values, so a closed season arrives
// without `isOpen` and an open season without `endDate`.
const normalizeSeason = (raw: Partial<Season>): Season => ({
  ...(raw as Season),
  isOpen: raw.isOpen ?? false,
});

const isNotFound = (error: unknown): boolean =>
  (error as { response?: { status?: number } } | null)?.response?.status ===
  404;

export const useCurrentSeason = () => {
  const { isLoggedIn, headers } = useHressAuth();

  return useQuery<Season | null>({
    queryKey: ["beer", "season", "current"],
    enabled: isLoggedIn,
    queryFn: async () => {
      try {
        const response = await axios.get<Partial<Season>>(
          hressApiUrl("beer/seasons/current"),
          { headers }
        );
        return normalizeSeason(response.data);
      } catch (error) {
        if (isNotFound(error)) return null;
        throw error;
      }
    },
  });
};
