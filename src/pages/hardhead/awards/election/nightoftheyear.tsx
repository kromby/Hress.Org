import { useState, useEffect } from "react";
import config from "react-global-configuration";
import axios from "axios";
import Post from "../../../../components/post";
import { useAuth } from "../../../../context/auth";
import HardheadRating from "../../components/rating";
import HardheadBody from "../../components/hardheadbody";
import { useNavigate, useLocation } from "react-router-dom";
import { useHardhead } from "../../../../hooks/hardhead/useHardhead";

interface MovieOfTheYearProps {
  ID: number;
  Name: string;
  Href: string;
  Description?: string;
  Date?: string;
  Year?: string;
  onSubmit: () => void;
}

const NightOfTheYear = ({
  ID,
  Name,
  Href,
  Description,
  Date,
  Year,
  onSubmit,
}: MovieOfTheYearProps) => {
  const { authTokens } = useAuth();
  const { fetchByHref } = useHardhead();
  const [nights, setNights] = useState<any[]>([]);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    const loadNights = async () => {
      const result = await fetchByHref(Href);
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
        description={Description}
        date={Date}
        dateFormatted={Year}
        body={
          <section>
            <p>
              Gefðu öllum kvöldunum sem þú mættir á einkunn, smelltu síðan á{" "}
              <i>Ljúka</i> neðst á síðunni til að halda áfram
            </p>
          </section>
        }
      />

      {nights
        ? nights.map((hardhead) => (
          <Post
            key={hardhead.id}
            id={hardhead.id}
            title={hardhead.name}
            description={`${hardhead.guestCount} gestir`}
            date={hardhead.date}
            dateFormatted={hardhead.dateString}
            author={hardhead.host}
            body={
              <HardheadBody
                id={hardhead.id}
                name={hardhead.name}
                description={hardhead.description}
                viewMovie={false}
                viewNight={true}
                viewGuests={true}
                movie={hardhead.movie}
                imageHeight={"270px"}
              />
            }
            actions={<ul className="actions" />}
            stats={
              <HardheadRating id={hardhead.id} nightRatingVisible="true" movieRatingVisible="false" />
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

export default NightOfTheYear;
