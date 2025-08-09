import { useState, useCallback } from "react";
import axios from "axios";
import config from "react-global-configuration";
import { MovieInfo } from "../../types/movieInfo";

interface UseMovieInfoResult {
  movieInfo: MovieInfo | null;
  loading: boolean;
  error: string | null;
  fetchMovieInfo: (movieId: number) => Promise<void>;
}

export const useMovieInfo = (): UseMovieInfoResult => {
  const [movieInfo, setMovieInfo] = useState<MovieInfo | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const fetchMovieInfo = useCallback(async (movieId: number) => {
    if (!movieId) {
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const url = `${config.get("apiPath")}/api/movies/${movieId}/info`;
      const response = await axios.get(url);
      setMovieInfo(response.data);
    } catch (err) {
      if (axios.isAxiosError(err)) {
        if (err.response?.status === 404) {
          // Movie info not found - this is acceptable, just means no detailed info available
          setError(null);
          setMovieInfo(null);
        } else {
          setError(`Failed to fetch movie info: ${err.message}`);
          console.error("[useMovieInfo] Error fetching movie info:", err);
        }
      } else {
        setError("An unexpected error occurred");
        console.error("[useMovieInfo] Unexpected error:", err);
      }
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    movieInfo,
    loading,
    error,
    fetchMovieInfo,
  };
};
