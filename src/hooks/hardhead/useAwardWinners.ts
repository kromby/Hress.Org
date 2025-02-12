import { useState, useEffect } from "react";
import config from "react-global-configuration";
import axios, { AxiosError } from "axios";
import { WinnerEntity } from "../../types/winnerEntity";

export const useAwardWinners = (position?: number) => {
  const [winners, setWinners] = useState<WinnerEntity>();
  const [error, setError] = useState<AxiosError | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    const fetchWinners = async () => {
      setIsLoading(true);
      try {
        const baseUrl = `${config.get(
          "apiPath"
        )}/api/hardhead/awards/364/winners`;
        const url =
          position !== undefined ? `${baseUrl}?position=${position}` : baseUrl;
        const response = await axios.get(url);
        setWinners(response.data[0]);
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
  }, [position]);

  return { winners, error, isLoading };
};
