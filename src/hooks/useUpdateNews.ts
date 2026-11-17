import axios from "axios";
import config from "react-global-configuration";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuth } from "../context/auth";
import { NewsEntity } from "../types/newsEntity";
import { CreateNewsRequest } from "./useCreateNews";

export const useUpdateNews = (id: number | undefined) => {
  const { authTokens } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (request: CreateNewsRequest): Promise<NewsEntity> => {
      const url = `${config.get("apiPath")}/api/news/${id}`;
      const response = await axios.put<NewsEntity>(url, request, {
        headers: { "X-Custom-Authorization": `token ${authTokens?.token}` },
      });
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["news"] });
    },
  });
};
