import axios from 'axios';
import config from 'react-global-configuration';

export const useHardhead = () => {
    const fetchByHref = async (href: string) => {
        const url = `${config.get("apiPath")}${href}`;
        const response = await axios.get(url);
        return response.data;
    };

    /*   const fetchById = async (id: string) => {
        const url = `${config.get("apiPath")}/nights/${id}`;
        const response = await axios.get(url);
        return response.data;
      };
    
      const fetchByParentId = async (parentId: string) => {
        const url = `${config.get("apiPath")}/nights/parent/${parentId}`;
        const response = await axios.get(url);
        return response.data;
      };
    
      const fetchByDates = async (startDate: string, endDate: string) => {
        const url = `${config.get("apiPath")}/nights/dates?startDate=${startDate}&endDate=${endDate}`;
        const response = await axios.get(url);
        return response.data;
      }; */

    return {
        fetchByHref,
        /*     fetchById,
            fetchByParentId,
            fetchByDates, */
    };
}; 