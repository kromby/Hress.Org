import { useState, useEffect } from "react";
import config from 'react-global-configuration';
import axios from 'axios';
import SidePost from "../../components/sidepost";

const YearsSide = ({year}) => {
    const [yearQS, setYearQS] = useState();
    const [years, setYears] = useState();
    const [lastUrl, setLastUrl] = useState();

    useEffect(() => {
        const getYears = async (url) => {
            
            try {
                const response = await axios.get(url);
                setYears(response.data);
            }
            catch (e) {
                console.error(e);
            }
        }
        
        let url = "";
        if (year) {
            url = config.get("apiPath") + "/api/news/statistics/years/" + year + "/months";   
            setYearQS(year);
        } else {
            url = config.get('apiPath') + "/api/news/statistics/years";
        }        

        if (!years || lastUrl !== url) {
            setLastUrl(url);
            getYears(url);
        }
    }, [year])

    return (
        <div>
            {years ? years.map((singleYear) =>
                <li key={singleYear.value}>
                    <SidePost
                        title={singleYear.count + " fréttir"}
                        dateString={yearQS ? singleYear.valueString.replace("YEAR", yearQS) : singleYear.valueString}
                        href={yearQS ? "/news/history?year=" + yearQS + "&month=" + singleYear.value : "/news/history?year=" + singleYear.value}
                    />
                </li>
            ) : null}
        </div>
    )
}

export default YearsSide;