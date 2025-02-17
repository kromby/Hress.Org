import { useState, useEffect } from "react";
import config from "react-global-configuration";
import axios, { AxiosError } from "axios";
import { WinnerEntity } from "../../types/winnerEntity";

import { useState, useEffect } from "react";
import axios, { AxiosError } from "axios";
import config from "config";

const useBaseAwardWinners = (
  baseUrl: string,
  userId?: number,
  position?: number,
  year?: number
) => {
  const [winners, setWinners] = useState<WinnerEntity[]>();
  const [error, setError] = useState<AxiosError | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    const fetchWinners = async () => {
      setIsLoading(true);
      try {
        const params = new URLSearchParams();
        if (userId) params.append("user", userId.toString());
        if (position) params.append("position", position.toString());
        if (year) params.append("year", year.toString());
        const url = `${baseUrl}${params.toString() ? `?${params.toString()}` : ""}`;

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
  }, [baseUrl, userId, position, year]);

  return { winners, error, isLoading };
};

export const useAwardWinnersById = (
  awardId: number,
  userId?: number,
  position?: number,
  year?: number
) => {
  const baseUrl = `${config.get("apiPath")}/api/hardhead/awards/${awardId}/winners`;
  return useBaseAwardWinners(baseUrl, userId, position, year);
};

export const useAwardWinnersByHref = (
  href: string,
  userId?: number,
  position?: number,
  year?: number
) => {
  const baseUrl = `${config.get("apiPath")}${href}`;
  return useBaseAwardWinners(baseUrl, userId, position, year);
};
