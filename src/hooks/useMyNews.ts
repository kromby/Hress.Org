import axios from "axios";
import config from "react-global-configuration";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../context/auth";
import { NewsEntity } from "../types/newsEntity";

export const useMyNews = () => {
  const { authTokens } = useAuth();
  const token: string | undefined = authTokens?.token;

  return useQuery<NewsEntity[]>({
    queryKey: ["news", "mine", token],
    queryFn: async (): Promise<NewsEntity[]> => {
      const url = `${config.get("apiPath")}/api/news/mine`;
      const response = await axios.get<NewsEntity[]>(url, {
        headers: { "X-Custom-Authorization": `token ${token}` },
      });
      return response.data ?? [];
    },
    enabled: Boolean(token),
  });
};
