import { useState, useEffect } from "react";
import config from "react-global-configuration";
import axios from "axios";
import Post from "../../../../components/post";
import { useAuth } from "../../../../context/auth";
import HardheadRating from "../../components/rating";
import HardheadBody from "../../components/hardheadbody";
import { useNavigate, useLocation } from "react-router-dom";
import { useHardhead } from "../../../../hooks/hardhead/useHardhead";
import { ElectionModuleProps } from ".";
import { HardheadNight } from "../../../../types/hardheadNight";

const MovieOfTheYear = ({
  ID,
  Name,
  Href,
  onSubmit,
}: ElectionModuleProps) => {
  const { authTokens } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const { fetchByHref } = useHardhead();
  const [nights, setNights] = useState<HardheadNight[]>([]);

  useEffect(() => {
    const loadNights = async () => {
      const result = await fetchByHref(Href || "");
      setNights(result);
    };
    loadNights();
  }, [Href]);

  const handleSubmit = async () => {
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
      return;
    }

    try {
      const url = `${config.get("apiPath")}/api/elections/${ID}/vote`;
      await axios.post(url, [], {
        headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
      });
    } catch (e) {
      console.error(e);
      // skipcq: JS-0052
      alert(e);
    }

    onSubmit();
  };

  return (
    <div>
      <Post
        id={ID}
        title={Name}
        body={
          <section>
            <p>
              Gefðu öllum myndunum sem þú sást einkunn, smelltu síðan á{" "}
              <b>Ljúka</b> neðst á síðunni til að halda áfram
            </p>
          </section>
        }
      />

      {nights
        ? nights.map((hardhead: HardheadNight) => (
          <Post
            key={hardhead.id}
            id={hardhead.id}
            title={hardhead.name}
            date={hardhead.date}
            dateFormatted={hardhead.dateString}
            body={
              <HardheadBody
                id={hardhead.id}
                name={hardhead.name}
                description={hardhead.description}
                viewMovie
                viewNight={false}
                viewGuests={false}
                imageHeight={"270px"}
                movie={hardhead.movie}
              />
            }
            actions={<ul className="actions" />}
            stats={
              <HardheadRating id={hardhead.id} nightRatingVisible={false} movieRatingVisible />
            }
          />
        ))
        : null}

      <ul className="actions pagination">
        <li>
          <button onClick={handleSubmit} className="button large next">
            {`Ljúka (${Name})`}
          </button>
        </li>
      </ul>
    </div>
  );
};

export default MovieOfTheYear;
