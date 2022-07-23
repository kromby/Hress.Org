import { useEffect, useState } from "react";
import {isMobile} from 'react-device-detect';
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

        document.title = 'Hress.Org';

        if(!news) {
            getNews();
        }
    }, [propsData])

    return (
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
                                <img style={{"maxHeight": "500px"}} src={config.get("apiPath") + news.image.href} alt={news.name} />
                            </span> : null }
                            <span dangerouslySetInnerHTML={{ __html: news.content }} />
                        </p>}
                    image={isMobile || news.imageAlign === 4 ? config.get('apiPath') + news.image.href : null}
                    actions = {<p />}
                />
            ) : null}
        </div>
    )
}

export default News;