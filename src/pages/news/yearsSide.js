import { useState, useEffect } from "react";
import config from 'react-global-configuration';
import axios from 'axios';
import SidePost from "../../components/sidepost";
import { useParams } from "react-router-dom-v5-compat";

const YearsSide = (propsData) => {
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
        
        console.log("[YearsSide] propsData.year");
        console.log(propsData.year);        
        if (propsData.year) {
            var url = config.get("apiPath") + "/api/news/statistics/years/" + propsData.year + "/months";   
            setYearQS(propsData.year);
        } else {
            var url = config.get('apiPath') + "/api/news/statistics/years";
        }        

        if (!years || lastUrl != url) {
            setLastUrl(url);
            getYears(url);
        }
    }, [propsData])

    return (
        <div>
            {years ? years.map((year) =>
                <li key={year.value}>
                    <SidePost
                        title={year.count + " fréttir"}
                        dateString={yearQS ? year.valueString.replace("YEAR", yearQS) : year.valueString}
                        href={yearQS ? "/news/history?year=" + yearQS + "&month=" + year.value : "/news/history?year=" + year.value}
                    />
                </li>
            ) : null}
        </div>
    )
}

export default YearsSide;