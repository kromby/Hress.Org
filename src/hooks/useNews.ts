import axios from "axios";
import config from "react-global-configuration";
import { useQuery } from "@tanstack/react-query";
import { NewsEntity } from "../types/newsEntity";

export const useNews = (id: number | undefined) =>
  useQuery<NewsEntity>({
    queryKey: ["news", id],
    queryFn: async (): Promise<NewsEntity> => {
      const url = `${config.get("apiPath")}/api/news/${id}`;
      const response = await axios.get<NewsEntity>(url);
      return response.data;
    },
    enabled: id !== undefined && id > 0,
    retry: false,
  });
