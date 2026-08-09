import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import axios, { AxiosError } from "axios";
import config from "react-global-configuration";
import { UserBasicEntity } from "../../types/legacy/userBasicEntity";

interface UseHardheadGuestsResult {
  guests: UserBasicEntity[] | undefined;
  loading: boolean;
  error: AxiosError | null;
  addGuest: (guestId: number) => Promise<void>;
  removeGuest: (guestId: number) => Promise<void>;
  refetch: () => Promise<void>;
}

export const useHardheadGuests = (
  hardheadID: number | string,
  token?: string
): UseHardheadGuestsResult => {
  const queryClient = useQueryClient();
  const queryKey = ["hardhead", hardheadID, "guests"];

  const fetchGuests = async (): Promise<UserBasicEntity[]> => {
    const url = `${config.get("apiPath")}/api/hardhead/${hardheadID}/guests`;
    const response = await axios.get<UserBasicEntity[]>(url);
    return response.data;
  };

  const {
    data: guests,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey,
    queryFn: fetchGuests,
    enabled: Boolean(hardheadID),
  });

  const addGuestMutation = useMutation({
    mutationFn: async (guestId: number) => {
      const url = `${config.get("apiPath")}/api/hardhead/${hardheadID}/guests/${guestId}`;
      await axios.post(
        url,
        {},
        {
          headers: token ? { "X-Custom-Authorization": `token ${token}` } : {},
        }
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey });
    },
  });

  const removeGuestMutation = useMutation({
    mutationFn: async (guestId: number) => {
      const url = `${config.get("apiPath")}/api/hardhead/${hardheadID}/guests/${guestId}`;
      await axios.delete(url, {
        headers: token ? { "X-Custom-Authorization": `token ${token}` } : {},
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey });
    },
  });

  return {
    guests,
    loading: isLoading,
    error: error as AxiosError | null,
    addGuest: async (guestId: number) => {
      await addGuestMutation.mutateAsync(guestId);
    },
    removeGuest: async (guestId: number) => {
      await removeGuestMutation.mutateAsync(guestId);
    },
    refetch: async () => {
      await refetch();
    },
  };
};
