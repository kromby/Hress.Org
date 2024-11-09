import { useState } from "react";
import axios from "axios";
import config from "react-global-configuration";
import { ImageContainer, ImageEntity } from "../types/image";
import { useAuth } from "../context/auth";

interface UploadImageParams {
  file?: File;
  source?: string;
  name: string;
  albumId: number;
}

export const useImages = () => {
  const { authTokens } = useAuth();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const postImage = async (imageEntity: Partial<ImageEntity>) => {
    const response = await axios.post(
      `${config.get("apiPath")}/api/images`,
      imageEntity,
      {
        headers: {
          "X-Custom-Authorization": `token ${authTokens.token}`,
          "Content-Type": "application/json",
        },
      }
    );

    if (response.status === 201) {
      return response.data;
    }
  };

  const uploadImage = async ({
    file,
    source,
    name,
    albumId,
  }: UploadImageParams) => {
    setLoading(true);
    setError(null);

    try {
      if (file) {
        // Convert file to base64
        const base64Content = await new Promise<string>((resolve, reject) => {
          const reader = new FileReader();
          reader.onload = () => {
            if (typeof reader.result === "string") {
              // Remove data:image/jpeg;base64, prefix
              const base64 = reader.result.split(",")[1];
              resolve(base64);
            }
          };
          reader.onerror = reject;
          reader.readAsDataURL(file);
        });

        // Create ImageEntity
        const imageEntity: Partial<ImageEntity> = {
          name,
          content: base64Content,
          container: ImageContainer.Album,
        };

        return await postImage(imageEntity);
      } else if (source) {
        // Create ImageEntity for URL-based upload
        const imageEntity: Partial<ImageEntity> = {
          name,
          photoUrl: source,
          container: ImageContainer.Album,
        };

        return await postImage(imageEntity);
      }
    } catch (err) {
      const message =
        err instanceof Error
          ? err.message
          : "Villa kom upp við að hlaða upp mynd";
      setError(message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  return {
    uploadImage,
    loading,
    error,
  };
};
