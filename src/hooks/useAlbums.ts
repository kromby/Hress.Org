import { useState } from "react";
import axios, { AxiosError } from "axios";
import config from "react-global-configuration";
import { useAuth } from "../context/auth";
import { AlbumEntity } from "../types/albumEntity";

interface UseAlbumsReturn {
  createAlbum: (albumData: AlbumEntity) => Promise<AlbumEntity>;
  getAlbum: (id: number) => Promise<AlbumEntity>;
  addImage: (albumId: number, imageId: number) => Promise<AlbumEntity>;
  loading: boolean;
  error: string | null;
}

export const useAlbums = (): UseAlbumsReturn => {
  const { authTokens } = useAuth();
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const createAlbum = async (albumData: AlbumEntity): Promise<AlbumEntity> => {
    setLoading(true);
    setError(null);

    try {
      const url = `${config.get("apiPath")}/api/albums`;
      const response = await axios.post<AlbumEntity>(url, albumData, {
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

  const getAlbum = async (id: number): Promise<AlbumEntity> => {
    setLoading(true);
    setError(null);

    try {
      const url = `${config.get("apiPath")}/api/albums/${id}`;
      const response = await axios.get<AlbumEntity>(url, {
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

  const addImage = async (
    albumId: number,
    imageId: number
  ): Promise<AlbumEntity> => {
    setLoading(true);
    setError(null);

    try {
      const url = `${config.get("apiPath")}/api/albums/${albumId}/images`;
      const response = await axios.post<AlbumEntity>(
        url,
        { imageId },
        {
          headers: {
            "X-Custom-Authorization": `token ${authTokens.token}`,
            "Content-Type": "application/json",
          },
        }
      );
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
    addImage,
    loading,
    error,
  };
};
