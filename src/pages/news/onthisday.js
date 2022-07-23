import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import { ErrorBoundary } from "react-error-boundary";
import axios from "axios";
import { MiniPost } from "../../components";
import { Link } from "react-router-dom";

const OnThisDay = () => {
    const [news, setNews] = useState();

    useEffect(() => {
        const getNews = async () => {

            var url = config.get("apiPath") + "/api/news/?type=onthisday&top=1";
            try {
                const response = await axios.get(url);
                setNews(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        if (!news) {
            getNews();
        }
    }, [])

    return (
        <ErrorBoundary
            FallbackComponent={<div>Oops!</div>}
            onError={(error, errorInfo) => errorService.log({ error, errorInfo })}
            onReset={() => {
                // reset the state of your app so the error doesn't happen again
            }}
        >
            <div>
                {news ? news.map(news =>
                    <MiniPost
                        key={news.id}
                        title={news.name}
                        description={
                            <Link to="/news/history">Sjá allar fréttir frá þessum degi</Link>
                        }
                        dateString={news.insertedString}
                        date={news.inserted}
                        href={"/news/" + news.id}
                        imageSource={news.image && news.image.id ? config.get('apiPath') + news.image.href : null}
                        imageHref={"/news/" + news.id}
                        userHref={"/gang/single.aspx?Id=" + news.author.id}
                        userPhoto={news.author.profilePhoto ? config.get('apiPath') + news.author.profilePhoto.href : null}
                        userText={news.author.username}
                    />) : null}
            </div>
        </ErrorBoundary>
    )
}

export default OnThisDay;