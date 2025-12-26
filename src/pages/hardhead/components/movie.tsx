import { useCallback, useState, useEffect } from "react";
import YouTube from "react-youtube";
import { Movie as MovieType } from "../../../types/movie";
import { useMovieInfo } from "../../../hooks/hardhead/useMovieInfo";

interface MovieProps {
  movie?: MovieType;
  id?: number;
  dateShown?: string;
}

const Movie = ({ movie, id, dateShown }: MovieProps) => {
  const [trailerOpen, setTrailerOpen] = useState<boolean>(false);
  const { movieInfo, fetchMovieInfo } = useMovieInfo();

  const opts = {
    // width: '800',
    playerVars: {
      // https://developers.google.com/youtube/player_parameters
      autoplay: 0,
    },
  };

  const toggleTrailer = useCallback(
    () => setTrailerOpen(!trailerOpen),
    [trailerOpen]
  );

  // Fetch detailed movie info after a short delay
  useEffect(() => {
    if (id && movie) {
      const timeoutId = setTimeout(() => {
        fetchMovieInfo(id);
      }, 500); // 500ms delay

      return () => clearTimeout(timeoutId);
    }

    return undefined;
  }, [id, movie, fetchMovieInfo]);

  // Calculate movie age when it was shown
  const calculateMovieAge = (): number | null => {
    if (!movieInfo?.year || !dateShown) return null;

    const hardheadDate = new Date(dateShown);
    const hardheadYear = hardheadDate.getFullYear();

    return hardheadYear - movieInfo.year;
  };

  const movieAge = calculateMovieAge();

  return movie ? (
    <section>
      <h3>Myndin</h3>
      <h4>
        <a href={movie.imdbUrl} target="_blank" rel="noopener noreferrer">
          {movie.name}
        </a>
      </h4>
      <span
        key="movie0"
        style={{
          color: "#999999",
          fontSize: "0.9em",
          display: "block",
          marginTop: "-0.5rem",
        }}
      >
        {movieInfo?.year ? `frá ${movieInfo.year}` : null}
        {movieAge !== null &&
          (movieAge === 0
            ? ", glæný"
            : `, ${movieAge} ${
                movieAge % 10 === 1 && movieAge % 100 !== 11 ? "árs" : "ára"
              } gömul`)}
        {movieInfo?.runtime ? ` - ${movieInfo?.runtime} mín` : null}
      </span>
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
      {movieInfo?.ratings ? <h4>Nánari upplýsingar</h4> : null}
      <div className="row" style={{ marginBottom: "1em" }}>
        {movieInfo?.genre?.length && movieInfo.genre.length > 0 ? (
          <div className="col-4">
            <strong>Tegund</strong>
            <br />
            {movieInfo.genre.join(", ")}
          </div>
        ) : null}
        {movieInfo?.language?.length && movieInfo.language.length > 0 ? (
          <div className="col-4">
            <strong>Tungumál</strong>
            <br />
            {movieInfo.language.join(", ")}
          </div>
        ) : null}
        {movieInfo?.country && (
          <div className="col-4">
            <strong>Land</strong>
            <br />
            {movieInfo.country}
          </div>
        )}
      </div>
      <div className="row" style={{ marginBottom: "1em" }}>
        {movieInfo?.awards ? (
          <div className="col-4">
            <strong>Verðlaun</strong>
            <br />
            {movieInfo.awards}
          </div>
        ) : null}
        {movieInfo?.boxOffice ? (
          <div className="col-4">
            <strong>Tekjur</strong>
            <br />
            {movieInfo.boxOffice}
          </div>
        ) : null}
      </div>
      {movieInfo?.ratings ? <h4>Einkunnir</h4> : null}
      <div className="row" style={{ marginBottom: "2em" }}>
        {movieInfo?.ratings && Object.keys(movieInfo.ratings).length > 0
          ? Object.entries(movieInfo.ratings).map(([key, value]) => (
              <div key={key} className="col-4">
                <strong>{key}</strong>
                <br />
                {value}
              </div>
            ))
          : null}
      </div>
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
