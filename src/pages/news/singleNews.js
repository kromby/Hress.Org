import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import axios from "axios";
import { Post } from "../../components";
import { Helmet } from "react-helmet";
import { useParams } from "react-router-dom";

const SingleNews = () => {
    const [news, setNews] = useState();
    const params = useParams();

    useEffect(() => {
        const getNews = async () => {

            var url = config.get("apiPath") + "/api/news/" + params.id;
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
        <div id="main">
            {news ?
                [
                    <Helmet key="helmet">
                        <title>{news.name} | Hress.Org</title>
                        <meta name="description" content={news.name} />
                        <meta property="og:title" content={news.name} />
                        <meta property="og:image" content={news.image && news.image.id ? config.get('apiPath') + news.image.href : null} />
                        <meta property="og:image:secure_url" content={news.image && news.image.id ? config.get('apiPath') + news.image.href : null} />
                        <meta property="og:image:width" content="1000" />
                        <meta property="og:image:height" content="563" />
                    </Helmet>,
                    <Post key={news.id}
                        id={news.id}
                        title={news.name}
                        date={news.inserted}
                        dateFormatted={news.insertedString}
                        author={news.author}
                        body={<span dangerouslySetInnerHTML={{ __html: news.content }} />}
                        image={news.image && news.image.id ? config.get('apiPath') + news.image.href + "?width=1400" : null}
                        actions={<p />}
                    />
                ] : null}
        </div>
    )
}

export default SingleNews;