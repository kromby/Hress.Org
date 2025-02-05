import { useState, useEffect, useMemo, useCallback } from 'react';
import axios, { AxiosError } from 'axios';
import config from "react-global-configuration";
import { Award } from '../../types/award';

export const useAwards = (year: number | undefined = undefined) => {
    const [awards, setAwards] = useState<Award[] | null>(null);
    const [error, setError] = useState<AxiosError | null>(null);
    const [isLoading, setIsLoading] = useState(false);

    const fetchAwards = useCallback(async () => {           
        setIsLoading(true);
        try {
            const url = `${config.get('apiPath')}/api/hardhead/awards${year ? `?year=${year}` : ''}`;
            const response = await axios.get(url);
            setAwards(response.data);
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
    }, [year]);

    useEffect(() => {
        fetchAwards();
    }, [fetchAwards]);

    // Memoize the return value to prevent unnecessary re-renders
    const returnValue = useMemo(() => ({
        awards,
        error,
        isLoading,
        refetch: fetchAwards
    }), [awards, error, isLoading, fetchAwards]);

    return returnValue;
}
