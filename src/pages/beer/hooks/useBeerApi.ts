import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import axios from "axios";
import { hressApiUrl, useHressAuth } from "../hress";
import { BeerSummary, Rating, Season } from "../types";

// The backend omits default and null values, so a closed season arrives
// without `isOpen` and an open season without `endDate`.
const normalizeSeason = (raw: Partial<Season>): Season => ({
  ...(raw as Season),
  isOpen: raw.isOpen ?? false,
});

const isNotFound = (error: unknown): boolean =>
  (error as { response?: { status?: number } } | null)?.response?.status ===
  404;

export const isUnauthorized = (error: unknown): boolean =>
  (error as { response?: { status?: number } } | null)?.response?.status ===
  401;

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

const seasonBeersKey = (seasonID?: string) => [
  "beer",
  "season",
  seasonID,
  "beers",
];

export const useSeasonBeers = (seasonID?: string) => {
  const { isLoggedIn, headers } = useHressAuth();

  return useQuery<BeerSummary[]>({
    queryKey: seasonBeersKey(seasonID),
    enabled: isLoggedIn && Boolean(seasonID),
    queryFn: async () => {
      const response = await axios.get<BeerSummary[]>(
        hressApiUrl(`beer/seasons/${encodeURIComponent(seasonID ?? "")}/beers`),
        { headers }
      );
      return response.data;
    },
  });
};

// Saves the current user's stars for a beer. Rating again replaces the earlier rating.
export const useSaveRating = (seasonID: string, beerID: string) => {
  const { headers } = useHressAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (stars: number): Promise<Rating> => {
      const response = await axios.put<Rating>(
        hressApiUrl(
          `beer/seasons/${encodeURIComponent(seasonID)}/beers/${encodeURIComponent(beerID)}/rating`
        ),
        { stars },
        { headers }
      );
      return response.data;
    },
    onSuccess: (rating) => {
      queryClient.setQueryData<BeerSummary[]>(
        seasonBeersKey(seasonID),
        (summaries) =>
          summaries?.map((s) =>
            s.beer.id === beerID ? { ...s, myRating: rating.stars } : s
          )
      );
      queryClient.invalidateQueries({ queryKey: seasonBeersKey(seasonID) });
    },
  });
};
