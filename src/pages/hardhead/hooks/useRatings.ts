import { useState, useEffect } from "react";
import axios from "axios";
import config from "react-global-configuration";
import { useAuth } from "../../../context/auth";
import { RatingsResponse } from "../../../types/ratings";

export const useRatings = (id: number) => {
  const { authTokens } = useAuth();
  const [ratings, setRatings] = useState<RatingsResponse>();

  const getRatingData = async () => {
    if (authTokens !== undefined) {
      try {
        const url = `${config.get("apiPath")}/api/hardhead/${id}/ratings`;
        const response = await axios.get<RatingsResponse>(url, {
          headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
        });
        setRatings(response.data);
      } catch (e) {
        console.error(e);
      }
    }
  };

  useEffect(() => {
    getRatingData();
  }, [id, authTokens]);

  return { ratings, refreshRatings: getRatingData };
};
