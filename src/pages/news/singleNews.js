import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import axios from "axios";
import { Post } from "../../components";

const SingleNews = (propsData) => {
    const [news, setNews] = useState();

    useEffect(() => {
        const getNews = async () => {

            var url = config.get("apiPath") + "/api/news/" + propsData.match.params.id;
            try {
                const response = await axios.get(url);
                setNews(response.data);                
            } catch(e) {
                console.error(e);
            }
        }

        getNews();
    }, [propsData])


    return (
        <div id="main">
            {news ? 
                <Post key={news.id}
                    id={news.id}
                    title={news.name}
                    date={news.inserted}
                    dateFormatted={news.insertedString}
                    author={news.author}
                    body={<span dangerouslySetInnerHTML={{ __html: news.content }} />}
                    image={news.image ? config.get('path') + news.image.href + "?code=" +  config.get('code') : null}
                    actions = {<p />}
                /> : null}
        </div>
    )
}

export default SingleNews;