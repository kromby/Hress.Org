import { useCallback, useState, useEffect } from "react";
import YouTube from "react-youtube";
import { Movie as MovieType } from "../../../types/movie";
import { useMovieInfo } from "../../../hooks/hardhead/useMovieInfo";
import MovieInfo from "./movieInfo";

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
      {movieInfo ? <MovieInfo movieInfo={movieInfo} /> : null}
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
