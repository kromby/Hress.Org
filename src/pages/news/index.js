import { useEffect, useState } from "react";
import {isMobile} from 'react-device-detect';
import axios from "axios";
import config from 'react-global-configuration';
import { Post } from "../../components";

const News = () => {
    const[news, setNews] = useState();    

    useEffect(() => {
        const getNews = async () => {
            const url = config.get("apiPath") + "/api/news";
            try {
                const response = await axios.get(url);
                setNews(response.data);
            }
            catch(e) {
                console.error(e);
            }
        }

        document.title = "Forsíða | Hress.Org";

        if(!news) {
            getNews();
        }
    }, [])

    return (
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
                        <p style={singleNews.image && singleNews.imageAlign !== 4 ? { "minHeight": singleNews.image.height-50 } : null}>
                            {!isMobile && singleNews.imageAlign !== 4 ?
                            <span className={singleNews.imageAlign === 1 ? "image left" : singleNews.imageAlign === 2 ? "image right" : null}>
                                <img style={{"maxHeight": "500px"}} src={config.get("apiPath") + singleNews.image.href} alt={singleNews.name} />
                            </span> : null }
                            {/* skipcq: JS-0440 */}
                            <span dangerouslySetInnerHTML={{ __html: singleNews.content }} />
                        </p>}
                    image={isMobile || singleNews.imageAlign === 4 ? config.get('apiPath') + singleNews.image.href + "?width=1400" : null}
                    actions = {<p />}
                />
            ) : null}
        </div>
    )
}

export default News;