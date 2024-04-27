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
        >
            <div>
                {news ? news.map(singleNews =>
                    <MiniPost
                        key={singleNews.id}
                        title={singleNews.name}
                        description={
                            <Link to="/news/history">Sjá allar fréttir frá þessum degi</Link>
                        }
                        dateString={singleNews.insertedString}
                        date={singleNews.inserted}
                        href={"/news/" + singleNews.id}
                        imageSource={singleNews.image?.id ? config.get('apiPath') + singleNews.image.href : null}
                        imageHref={"/news/" + singleNews.id}
                        userHref={"/gang/single.aspx?Id=" + singleNews.author.id}
                        userPhoto={singleNews.author.profilePhoto ? config.get('apiPath') + singleNews.author.profilePhoto.href : null}
                        userText={singleNews.author.username}
                    />) : null}
            </div>
        </ErrorBoundary>
    )
}

export default OnThisDay;