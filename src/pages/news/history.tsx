import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import axios from "axios";
import NewsPost from "./components/newsPost";
import queryString from 'query-string';
import { useLocation } from "react-router-dom";
import { NewsEntity } from "../../types/newsEntity";

const HistoryNews = () => {
    const [news, setNews] = useState<NewsEntity[]>();
    const [lastUrl, setLastUrl] = useState<string>();
    const location = useLocation();

    useEffect(() => {
        const getNews = async (url: string) => {
            try {
                const response = await axios.get(url);
                setNews(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        const parsed = queryString.parse(location.search);
        let url = "";
        if (parsed.year) {
            if(parsed.month) {
                url = config.get("apiPath") + "/api/news/?year=" + parsed.year + "&month=" + parsed.month;   
            } else {            
                url = config.get("apiPath") + "/api/news/?year=" + parsed.year;   
            }
            document.title = "Fréttir ársins " + parsed.year + " | Hress.Org";         
        } else {
            url = config.get("apiPath") + "/api/news/?type=onthisday";
            document.title = "Gamlar fréttir | Hress.Org";
        }        

        if (!news || lastUrl !== url) {
            setLastUrl(url);
            getNews(url);
        }
    }, [location])


    return (
            <div id="main">
                {news ? news.map((singleNews: NewsEntity) =>
                    <NewsPost key={singleNews.id} singleNews={singleNews} />) : null}
            </div>
    )
}

export default HistoryNews;