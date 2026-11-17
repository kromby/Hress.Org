import axios from "axios";
import config from "react-global-configuration";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../context/auth";
import { AlbumEntity } from "../types/albumEntity";
import { ImageEntity } from "../types/image";

export const useAlbumList = () => {
  const { authTokens } = useAuth();

  return useQuery<AlbumEntity[]>({
    queryKey: ["albums"],
    queryFn: async (): Promise<AlbumEntity[]> => {
      const url = `${config.get("apiPath")}/api/albums`;
      const response = await axios.get<AlbumEntity[]>(url, {
        headers: { "X-Custom-Authorization": `token ${authTokens?.token}` },
      });
      return response.data;
    },
    enabled: authTokens !== undefined,
  });
};

export const useAlbumImages = (albumId: number | undefined) => {
  const { authTokens } = useAuth();

  return useQuery<ImageEntity[]>({
    queryKey: ["albums", albumId, "images"],
    queryFn: async (): Promise<ImageEntity[]> => {
      const url = `${config.get("apiPath")}/api/albums/${albumId}/images`;
      const response = await axios.get<ImageEntity[]>(url, {
        headers: { "X-Custom-Authorization": `token ${authTokens?.token}` },
      });
      return response.data;
    },
    enabled: albumId !== undefined && albumId > 0 && authTokens !== undefined,
  });
};
