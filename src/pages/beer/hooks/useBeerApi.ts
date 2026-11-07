import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import axios from "axios";
import { hressApiUrl, useHressAuth } from "../hress";
import {
  Beer,
  BeerSummary,
  CatalogBeer,
  ChatMessage,
  NewBeerInput,
  Rating,
  Season,
} from "../types";

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

export const seasonBeersKey = (seasonID?: string) => [
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
    // Saves for the same beer run one at a time, in the order they were made,
    // so the last tap is the rating that's kept.
    scope: { id: `beer-rating:${seasonID}:${beerID}` },
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

// Not under seasonBeersKey, so saving a rating doesn't refetch the chat.
export const messagesKey = (seasonID: string, beerID: string) => [
  "beer",
  "season",
  seasonID,
  "messages",
  beerID,
];

// The beer's chat, oldest first. Empty until someone posts.
export const useMessages = (seasonID: string, beerID: string) => {
  const { isLoggedIn, headers } = useHressAuth();

  return useQuery<ChatMessage[]>({
    queryKey: messagesKey(seasonID, beerID),
    enabled: isLoggedIn,
    queryFn: async () => {
      const response = await axios.get<ChatMessage[]>(
        hressApiUrl(
          `beer/seasons/${encodeURIComponent(seasonID)}/beers/${encodeURIComponent(beerID)}/messages`
        ),
        { headers }
      );
      return response.data;
    },
  });
};

// Posts a plain-text message to the beer's chat. The first message also
// creates the beer's chat thread.
export const usePostMessage = (seasonID: string, beerID: string) => {
  const { headers } = useHressAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (text: string): Promise<ChatMessage> => {
      const response = await axios.post<ChatMessage>(
        hressApiUrl(
          `beer/seasons/${encodeURIComponent(seasonID)}/beers/${encodeURIComponent(beerID)}/messages`
        ),
        { text },
        { headers }
      );
      return response.data;
    },
    onSuccess: (message) => {
      // Show the new message right away, then refetch to pick up others' messages.
      queryClient.setQueryData<ChatMessage[]>(
        messagesKey(seasonID, beerID),
        (messages) => [...(messages ?? []), message]
      );
      queryClient.invalidateQueries({
        queryKey: messagesKey(seasonID, beerID),
      });
    },
  });
};

// Searches the beer catalog by name. Waits for at least two characters.
export const useCatalogSearch = (query: string) => {
  const { headers } = useHressAuth();
  const trimmed = query.trim();

  return useQuery<CatalogBeer[]>({
    queryKey: ["beer", "catalog", trimmed],
    enabled: trimmed.length >= 2,
    queryFn: async () => {
      const response = await axios.get<CatalogBeer[]>(
        hressApiUrl(`beer/beers?query=${encodeURIComponent(trimmed)}`),
        { headers }
      );
      return response.data;
    },
  });
};

// Adds a catalog beer to the season. The variable is the beer ID.
export const useAddBeerToSeason = (seasonID: string) => {
  const { headers } = useHressAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (beerID: string): Promise<Beer> => {
      const response = await axios.post<Beer>(
        hressApiUrl(`beer/seasons/${encodeURIComponent(seasonID)}/beers`),
        { beerID },
        { headers }
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: seasonBeersKey(seasonID) });
    },
  });
};

// Thrown when the beer is already in the catalog (same name and producer).
export class DuplicateBeerError extends Error {
  constructor(public existing: Beer) {
    super(`The beer '${existing.name}' already exists.`);
    Object.setPrototypeOf(this, DuplicateBeerError.prototype);
    this.name = "DuplicateBeerError";
  }
}

// Creates a new beer in the catalog and adds it to the season.
export const useCreateBeer = (seasonID: string) => {
  const { headers } = useHressAuth();
  const queryClient = useQueryClient();

  return useMutation<Beer, unknown, NewBeerInput>({
    mutationFn: async (input) => {
      try {
        const response = await axios.post<Beer>(
          hressApiUrl(`beer/seasons/${encodeURIComponent(seasonID)}/beers`),
          { beer: input },
          { headers }
        );
        return response.data;
      } catch (error) {
        const response = (
          error as {
            response?: { status?: number; data?: { duplicate?: Beer } };
          } | null
        )?.response;
        if (response?.status === 409 && response.data?.duplicate) {
          throw new DuplicateBeerError(response.data.duplicate);
        }
        throw error;
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: seasonBeersKey(seasonID) });
    },
  });
};
