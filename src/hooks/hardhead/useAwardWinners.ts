import { useState, useEffect } from "react";
import config from "react-global-configuration";
import axios, { AxiosError } from "axios";
import { WinnerEntity } from "../../types/winnerEntity";

export const useAwardWinners = (
  awardId: number,
  userId?: number,
  position?: number
) => {
  const [winners, setWinners] = useState<WinnerEntity[]>();
  const [error, setError] = useState<AxiosError | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    const fetchWinners = async () => {
      setIsLoading(true);
      try {
        const baseUrl = `${config.get(
          "apiPath"
        )}/api/hardhead/awards/${awardId}/winners`;
        const params = new URLSearchParams();
        if (userId) params.append("user", userId.toString());
        if (position !== undefined)
          params.append("position", position.toString());
        const url = `${baseUrl}${
          params.toString() ? `?${params.toString()}` : ""
        }`;

        const response = await axios.get(url);
        setWinners(response.data);
      } catch (e) {
        if (axios.isAxiosError(e)) {
          console.error("Failed to fetch winners:", e.message);
          setError(e);
        } else {
          console.error("An unexpected error occurred:", e);
          setError(new AxiosError("An unexpected error occurred"));
        }
      } finally {
        setIsLoading(false);
      }
    };

    fetchWinners();
  }, [awardId, userId, position]);

  return { winners, error, isLoading };
};
