import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import {isMobile} from 'react-device-detect';
import { ErrorBoundary } from "react-error-boundary";
import axios from "axios";
import { Post } from "../../components";

const HistoryNews = (propsData) => {
    const [news, setNews] = useState();

    useEffect(() => {
        const getNews = async () => {
            var url = config.get("apiPath") + "/api/news/?type=onthisday";
            try {
                const response = await axios.get(url);
                setNews(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        document.title = "Gamlar fréttir | Hress.Org";

        if (!news) {
            getNews();
        }
    }, [propsData])


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
                            <p style={news.image && news.imageAlign !== 4 ? { "minHeight": news.image.height-50 } : null}>
                                {!isMobile && news.imageAlign != 4 ?
                                    <span className={news.imageAlign === 1 ? "image left" : news.imageAlign === 2 ? "image right" : null}>
                                        <img style={{ "maxHeight": "500px" }} src={config.get("apiPath") + news.image.href} alt={news.name} />
                                    </span> : null}
                                <span dangerouslySetInnerHTML={{ __html: news.content }} />
                            </p>}
                        image={isMobile || news.imageAlign === 4 ? config.get('apiPath') + news.image.href : null}
                        actions={<p />}
                    />) : null}
            </div>
        </ErrorBoundary>
    )
}

export default HistoryNews;