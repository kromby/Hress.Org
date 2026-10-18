import config from 'react-global-configuration';
import { Post } from "../../components";
import { Link, useParams } from "react-router-dom";
import { useNews } from "../../hooks/useNews";
import { sanitizeNewsHtml } from "../../utils/sanitizeHtml";

const isAuthor = (news) =>
    String(news.insertedBy ?? news.author?.id) === localStorage.getItem("userID");

const SingleNews = () => {
    const params = useParams();
    const { data: news } = useNews(Number(params.id));

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
                        body={<span dangerouslySetInnerHTML={{ __html: sanitizeNewsHtml(news.content) }} />}
                        image={news.image?.id ? config.get('apiPath') + news.image.href + "?width=1400" : null}
                        actions={isAuthor(news) ?
                            <ul className="actions">
                                <li><Link to={`/news/${news.id}/edit`} className="button">Breyta</Link></li>
                            </ul>
                            : <p />}
                    />
                :null}
        </div>
    )
}

export default SingleNews;