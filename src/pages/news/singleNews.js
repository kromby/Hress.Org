import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import axios from "axios";
import { Post } from "../../components";
import { useParams } from "react-router-dom";

const SingleNews = () => {
    const [news, setNews] = useState();
    const params = useParams();

    useEffect(() => {
        const getNews = async () => {

            const url = config.get("apiPath") + "/api/news/" + params.id;
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
                    <Post key={news.id}
                        id={news.id}
                        title={news.name}
                        date={news.inserted}
                        dateFormatted={news.insertedString}
                        author={news.author}
                        // skipcq: JS-0440
                        body={<span dangerouslySetInnerHTML={{ __html: news.content }} />}
                        image={news.image?.id ? config.get('apiPath') + news.image.href + "?width=1400" : null}
                        actions={<p />}
                    />
                :null}
        </div>
    )
}

export default SingleNews;