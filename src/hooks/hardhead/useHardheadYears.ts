import { useState, useEffect } from "react";
import axios, { AxiosError } from "axios";
import config from "react-global-configuration";
import { YearEntity } from "../../types/yearEntity";

interface UseHardheadYearsReturn {
  years: YearEntity[];
  error: AxiosError | null;
  isLoading: boolean;
}

export const useHardheadYears = (): UseHardheadYearsReturn => {
  const [years, setYears] = useState<YearEntity[]>([]);
  const [error, setError] = useState<AxiosError | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    const fetchYears = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const url = `${config.get("apiPath")}/api/hardhead/years`;
        const response = await axios.get(url);
        setYears(response.data);
      } catch (e) {
        if (axios.isAxiosError(e)) {
          console.error("Failed to fetch years:", e.message);
          setError(e);
        } else {
          console.error("An unexpected error occurred:", e);
          setError(new AxiosError("An unexpected error occurred"));
        }
      } finally {
        setIsLoading(false);
      }
    };

    fetchYears();
  }, []);

  return { years, error, isLoading };
};
