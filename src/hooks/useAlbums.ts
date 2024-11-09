import { useState } from "react";
import axios, { AxiosError } from "axios";
import config from "react-global-configuration";
import { useAuth } from "../context/auth";
import { Album } from "../types/album";

interface UseAlbumsReturn {
  createAlbum: (albumData: Album) => Promise<Album>;
  getAlbum: (id: number) => Promise<Album>;
  loading: boolean;
  error: string | null;
}

export const useAlbums = (): UseAlbumsReturn => {
  const { authTokens } = useAuth();
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const createAlbum = async (albumData: Album): Promise<Album> => {
    setLoading(true);
    setError(null);

    try {
      const url = `${config.get("apiPath")}/api/albums`;
      const response = await axios.post<Album>(url, albumData, {
        headers: {
          "X-Custom-Authorization": `token ${authTokens.token}`,
          "Content-Type": "application/json",
        },
      });
      return response.data;
    } catch (err) {
      const error = err as AxiosError;
      const errorMessage =
        (error.response?.data as { error?: string })?.error || error.message;
      setError(errorMessage);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const getAlbum = async (id: number): Promise<Album> => {
    setLoading(true);
    setError(null);

    try {
      const url = `${config.get("apiPath")}/api/albums/${id}`;
      const response = await axios.get<Album>(url, {
        headers: {
          "X-Custom-Authorization": `token ${authTokens.token}`,
        },
      });
      return response.data;
    } catch (err) {
      const error = err as AxiosError;
      const errorMessage =
        (error.response?.data as { error?: string })?.error || error.message;
      setError(errorMessage);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  return {
    createAlbum,
    getAlbum,
    loading,
    error,
  };
};
