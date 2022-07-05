import { useEffect, useState } from "react";
import axios from "axios";
import config from 'react-global-configuration';
import { Post } from "../../components";

const News = (propsData) => {
    const[news, setNews] = useState();    

    useEffect(() => {
        const getNews = async () => {
            var url = config.get("apiPath") + "/api/news";
            try {
                const response = await axios.get(url);
                setNews(response.data);
            }
            catch(e) {
                console.error(e);
            }
        }

        if(!news) {
            getNews();
        }
    }, [propsData])

    return (
        <div id="main">
            {news ? news.map(news => 
                <Post key={news.id}
                    id={news.id}
                    title={news.name}
                    date={news.inserted}
                    dateFormatted={news.insertedString}
                    author={news.author}
                    body={news.content}
                    image={config.get('path') + news.image.href + "?code=" +  config.get('code')}
                />
            ) : null}
        </div>
    )
}

export default News;