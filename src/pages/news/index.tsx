import { useEffect, useState } from "react";
import axios from "axios";
import config from "react-global-configuration";
import NewsPost from "./components/newsPost";
import { NewsEntity } from "../../types/newsEntity";

const News = () => {
  const [news, setNews] = useState<NewsEntity[]>();

  useEffect(() => {
    const getNews = async () => {
      const url = `${config.get("apiPath")}/api/news`;
      try {
        const response = await axios.get(url);
        setNews(response.data);
      } catch (e) {
        console.error(e);
      }
    };

    document.title = "Forsíða | Hress.Org";

    if (!news) {
      getNews();
    }
  }, []);

  return (
    <div id="main">
      {news
        ? news.map((singleNews: NewsEntity) => (
            <NewsPost
              key={singleNews.id}
              singleNews={singleNews}
            />
          ))
        : null}
    </div>
  );
};

export default News;
