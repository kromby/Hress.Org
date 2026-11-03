import { Link, useParams } from "react-router-dom";
import { useNews } from "../../hooks/useNews";
import NewsPost from "./components/newsPost";

const isAuthor = (news) =>
    String(news.insertedBy ?? news.author?.id) === localStorage.getItem("userID");

const SingleNews = () => {
    const params = useParams();
    const { data: news } = useNews(Number(params.id));

    return (
        <div id="main">
            {news ?
                    <NewsPost key={news.id}
                        singleNews={news}
                        linked={false}
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
