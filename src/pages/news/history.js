import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import { isMobile } from 'react-device-detect';
import { ErrorBoundary } from "react-error-boundary";
import axios from "axios";
import { Post } from "../../components";
import queryString from 'query-string';
import { useLocation } from "react-router-dom";

const HistoryNews = () => {
    const [news, setNews] = useState();
    const [lastUrl, setLastUrl] = useState();
    const location = useLocation();

    useEffect(() => {
        const getNews = async (url) => {
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
        <ErrorBoundary
            FallbackComponent={<div>Oops!</div>}
        >
            <div id="main">
                {news ? news.map(singleNews =>
                    <Post key={singleNews.id}
                        id={singleNews.id}
                        href={"/news/" + singleNews.id}
                        title={singleNews.name}
                        date={singleNews.inserted}
                        dateFormatted={singleNews.insertedString}
                        author={singleNews.author}
                        body={
                            <p style={singleNews.image && singleNews.imageAlign !== 4 ? { "minHeight": singleNews.image.height - 50 } : null}>
                                {!isMobile && singleNews.image && singleNews.imageAlign !== 4 ?
                                    <span className={singleNews.imageAlign === 1 ? "image left" : singleNews.imageAlign === 2 ? "image right" : null}>
                                        <img style={{ "maxHeight": "500px" }} src={config.get("apiPath") + singleNews.image.href} alt={singleNews.name} />
                                    </span> : null}                                
                                {/* skipcq: JS-0440 */}
                                <span dangerouslySetInnerHTML={{ __html: singleNews.content }} />                                
                            </p>}
                        image={isMobile || singleNews.imageAlign === 4 ? config.get('apiPath') + singleNews.image.href + "?width=1400": null}
                        actions={<p />}
                    />) : null}
            </div>
        </ErrorBoundary>
    )
}

export default HistoryNews;