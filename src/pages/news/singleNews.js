import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import axios from "axios";
import { Post } from "../../components";
import { Helmet } from "react-helmet";

const SingleNews = (propsData) => {
    const [news, setNews] = useState();

    useEffect(() => {
        const getNews = async () => {

            var url = config.get("apiPath") + "/api/news/" + propsData.match.params.id;
            try {
                const response = await axios.get(url);
                setNews(response.data);

                // document.title = response.data.name + " | Hress.Org";
            } catch (e) {
                console.error(e);
            }
        }

        getNews();
    }, [propsData])


    return (
        <div id="main">


            {news ?
                [
                    <Helmet>
                        <title>{news.name} | Hress.Org</title>
                        <meta name="description" content={news.name} />
                        <meta property="og:title" content={news.name}></meta>
                        <meta property="og:image" content={news.image ? config.get('path') + news.image.href + "?code=" + config.get('code') : null}></meta>
                        <meta property="og:image:secure_url" content={news.image ? config.get('path') + news.image.href + "?code=" + config.get('code') : null}></meta>
                        <meta property="og:image:width" content="1000"></meta>
                        <meta property="og:image:height" content="563"></meta>
                        {/* <meta property="og:description" content=""></meta> */}
                    </Helmet>,
                    <Post key={news.id}
                        id={news.id}
                        title={news.name}
                        date={news.inserted}
                        dateFormatted={news.insertedString}
                        author={news.author}
                        body={<span dangerouslySetInnerHTML={{ __html: news.content }} />}
                        image={news.image ? config.get('path') + news.image.href + "?code=" + config.get('code') : null}
                        actions={<p />}
                    />
                ] : null}
        </div>
    )
}

export default SingleNews;