import axios from "axios";
import config from "react-global-configuration";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuth } from "../context/auth";
import { NewsEntity } from "../types/newsEntity";

export type CreateNewsRequest = {
  name: string;
  content: string;
  public: boolean;
  imageId?: number;
  imageAlign?: number;
};

export const useCreateNews = () => {
  const { authTokens } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (request: CreateNewsRequest): Promise<NewsEntity> => {
      const url = `${config.get("apiPath")}/api/news`;
      const response = await axios.post<NewsEntity>(url, request, {
        headers: { "X-Custom-Authorization": `token ${authTokens?.token}` },
      });
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["news"] });
    },
  });
};
