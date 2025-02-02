import { useState, useEffect } from 'react';
import axios, { AxiosError } from 'axios';
import config from "react-global-configuration";
import { Award } from '../types/award';

export const useAward = (awardId: number) => {
    const [award, setAward] = useState<Award | null>(null);
    const [error, setError] = useState<AxiosError | null>(null);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        const fetchAward = async () => {
            if (!awardId) return;
            
            setIsLoading(true);
            try {
                const url = `${config.get('apiPath')}/api/hardhead/awards/${awardId}`;
                const response = await axios.get(url);
                setAward(response.data);
            } catch (e) {
                if (axios.isAxiosError(e)) {
                    console.error("Failed to fetch award:", e.message);
                    setError(e);
                  } else {
                    console.error("An unexpected error occurred:", e);
                    setError(new AxiosError("An unexpected error occurred"));
                  }
            } finally {
                setIsLoading(false);
            }
        };

        fetchAward();
    }, [awardId]);

    return { award, error, isLoading };
};
