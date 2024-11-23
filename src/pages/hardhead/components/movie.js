import { useCallback, useState } from "react";
import YouTube from "react-youtube";

const Movie = ({ movie }) => {
  const [trailerOpen, setTrailerOpen] = useState(false);

  const opts = {
    // width: '800',
    playerVars: {
      // https://developers.google.com/youtube/player_parameters
      autoplay: 0,
    },
  };

  const toggleTrailer = useCallback(() => setTrailerOpen(!trailerOpen));

  return movie ? (
    <section>
      <h3>Myndin</h3>
      <h4>
        <a href={movie.imdbUrl} target="_blank" rel="noopener noreferrer">
          {movie.name}
        </a>
      </h4>
      <p key="movie1">
        {movie.actor}
        {movie.hardheadKillCount || movie.movieKillCount ? " (" : null}
        {movie.hardheadKillCount ? (
          <span>bar ábyrgð á {movie.hardheadKillCount} drápum </span>
        ) : null}
        {movie.movieKillCount ? `af ${movie.movieKillCount}` : null}
        {movie.hardheadKillCount || movie.movieKillCount
          ? " í myndinni)"
          : null}
      </p>
      <p key="movie2">
        {movie.reason?.trim()
          ? movie.reason
          : "Gestgjafi hefur ekki skráð ástæðu fyrir mynd :("}
      </p>
      {movie.youtubeUrl ? (
        <p key="movie3">
          <button className="button small" onClick={toggleTrailer}>
            {trailerOpen ? "Fela trailer" : "Sjá trailer"}
          </button>
        </p>
      ) : null}
      {trailerOpen ? (
        <YouTube key="movie4" videoId={movie.youtubeUrl} opts={opts} />
      ) : null}
    </section>
  ) : null;
};

export default Movie;
