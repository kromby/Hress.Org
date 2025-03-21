import { useEffect, useState } from "react";
import config from "react-global-configuration";
import Guests from "./guests";
import Movie from "./movie";

const HardheadBody = ({
  id,
  name,
  description,
  viewNight,
  viewMovie,
  viewGuests,
  imageHeight,
  movie,
}) => {
  const [showNight, setShowNight] = useState(true);
  const [showMovie, setShowMovie] = useState(true);
  const [showGuests, setShowGuests] = useState(true);

  useEffect(() => {
    if (viewNight !== undefined) {
      setShowNight(viewNight);
    }

    if (viewMovie !== undefined) {
      setShowMovie(viewMovie);
    }

    if (viewGuests !== undefined) {
      setShowGuests(viewGuests);
    }
  }, [viewNight, viewMovie, viewGuests]);

  return [
    movie?.posterPhoto?.href ? (
      <span key="A" className="image right">
        <img
          src={config.get("apiPath") + movie.posterPhoto.href}
          alt={name}
          style={{ height: imageHeight }}
        />
      </span>
    ) : null,
    showNight ? (
      <section key="0">
        <h3>Kvöldið</h3>
        <p>
          {description
            ? description
            : "Líklega hefur ekkert merkilegt gerst fyrst gestgjafi hefur ekki skráð neitt."}
        </p>
      </section>
    ) : null,
    showMovie ? <Movie key="movie" id={id} movie={movie} /> : null,
    showGuests ? <Guests key="2" hardheadID={id} /> : null,
    <p key="3" />,
  ];
};

export default HardheadBody;
