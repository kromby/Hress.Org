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
        if (parsed.year) {
            if(parsed.month) {
                var url = config.get("apiPath") + "/api/news/?year=" + parsed.year + "&month=" + parsed.month;   
            } else {            
                var url = config.get("apiPath") + "/api/news/?year=" + parsed.year;   
            }
            document.title = "Fréttir ársins " + parsed.year + " | Hress.Org";         
        } else {
            var url = config.get("apiPath") + "/api/news/?type=onthisday";
            document.title = "Gamlar fréttir | Hress.Org";
        }        

        if (!news || lastUrl != url) {
            setLastUrl(url);
            getNews(url);
        }
    }, [location])


    return (
        <ErrorBoundary
            FallbackComponent={<div>Oops!</div>}
            onError={(error, errorInfo) => errorService.log({ error, errorInfo })}
            onReset={() => {
                // reset the state of your app so the error doesn't happen again
            }}
        >
            <div id="main">
                {news ? news.map(news =>
                    <Post key={news.id}
                        id={news.id}
                        href={"/news/" + news.id}
                        title={news.name}
                        date={news.inserted}
                        dateFormatted={news.insertedString}
                        author={news.author}
                        body={
                            <p style={news.image && news.imageAlign !== 4 ? { "minHeight": news.image.height - 50 } : null}>
                                {!isMobile && news.image && news.imageAlign != 4 ?
                                    <span className={news.imageAlign === 1 ? "image left" : news.imageAlign === 2 ? "image right" : null}>
                                        <img style={{ "maxHeight": "500px" }} src={config.get("apiPath") + news.image.href} alt={news.name} />
                                    </span> : null}
                                <span dangerouslySetInnerHTML={{ __html: news.content }} />
                            </p>}
                        image={isMobile || news.imageAlign === 4 ? config.get('apiPath') + news.image.href + "?width=1400": null}
                        actions={<p />}
                    />) : null}
            </div>
        </ErrorBoundary>
    )
}

export default HistoryNews;