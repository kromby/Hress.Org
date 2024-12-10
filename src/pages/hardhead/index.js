import { useEffect, useState } from "react";
import config from "react-global-configuration";
import { Post } from "../../components";
import HardheadRating from "./components/rating";
import queryString from "query-string";
import HardheadActions from "./components/actions";
import axios from "axios";
import HardheadBody from "./components/hardheadbody";
import VoteNow from "./awards/election/votenow";
import { useLocation, useParams } from "react-router-dom";

const Hardhead = () => {
  const location = useLocation();
  const params = useParams();
  const [hardheads, setHardheads] = useState();
  const [lastUrl, setLastUrl] = useState();

  useEffect(() => {
    const getHardheads = async (url) => {
      try {
        const response = await axios.get(url);
        if (Array.isArray(response.data)) {
          setHardheads(response.data);
        } else {
          setHardheads([response.data]);
        }
      } catch (e) {
        console.error(e);
      }
    };

    const getHardheadsUrl = () => {
      const parsed = queryString.parse(location.search);
      let url = "";
      if (params.hardheadID) {
        url = `${config.get("apiPath")}/api/hardhead/${params.hardheadID}`;
      } else if (parsed.parentID) {
        url = `${config.get("apiPath")}/api/hardhead?parentID=${
          parsed.parentID
        }`;
      } else if (parsed.userID) {
        url = `${config.get("apiPath")}/api/hardhead?userID=${parsed.userID}`;
      } else if (parsed.query) {
        url = `${config.get("apiPath")}/api/hardhead?filter=${parsed.query}`;
      } else {
        const currentDate = new Date();
        currentDate.setMonth(currentDate.getMonth() - 5);
        url = `${config.get("apiPath")}/api/hardhead?dateFrom=${
          currentDate.getMonth() + 1
        }.1.${currentDate.getFullYear()}`;
      }

      return url;
    };

    document.title = "Harðhaus | Hress.Org";

    const url = getHardheadsUrl();

    if (!hardheads || lastUrl !== url) {
      getHardheads(url);
      setLastUrl(url);
    }
  }, [location]);

  return (
    <div id="main">
      <VoteNow />

      {hardheads
        ? hardheads.map((hardhead) => (
            <Post
              key={hardhead.id}
              id={hardhead.id}
              href={`/hardhead/${hardhead.id}`}
              title={hardhead.name}
              description={
                hardhead.guestCount ? `${hardhead.guestCount} gestir` : null
              }
              date={hardhead.date}
              dateFormatted={hardhead.dateString}
              author={hardhead.host}
              userPath="/hardhead/users/"
              body={
                <HardheadBody
                  id={hardhead.id}
                  name={hardhead.name}
                  description={hardhead.description}
                  movie={hardhead.movie}
                />
              }
              actions={<HardheadActions id={hardhead.id} />}
              stats={
                <HardheadRating
                  id={hardhead.id}
                  movieRatingVisible
                  nightRatingVisible
                />
              }
            />
          ))
        : null}
    </div>
  );
};

export default Hardhead;
