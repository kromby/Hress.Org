import { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import YouTube from 'react-youtube';

const Movie = ({id, photoPostback}) => {
    const [movieData, setMovieData] = useState({ movie: null })
    const [trailerOpen, setTrailerOpen] = useState(false);

    const opts = {
        // width: '800',
        playerVars: {
            // https://developers.google.com/youtube/player_parameters
            autoplay: 0,
        }
    };

    const movieUrl = config.get("path") + "/api/movies/" + id + "?code=" + config.get("code");

    useEffect(() => {
        const getMovieData = async () => {
            try {
                const response = await axios.get(movieUrl);

                if (response.data !== undefined) {
                    setMovieData({ movie: response.data });
                    if (response.data.PosterPhoto) {
                        photoPostback(response.data.PosterPhoto.Href);
                    }
                }
            }
            catch (e) {
                if (e.response === undefined || e.response.status !== 404) {
                    console.error(e);
                }
            }
        };

        if (!movieData.movie) {
            getMovieData();
        }
    }, [id, photoPostback, movieUrl])

    const toggleTrailer = async () => {
        setTrailerOpen(!trailerOpen);
    }

    return (
        movieData.movie ?
            <section>
                <h3>Myndin</h3>
                <h4><a href={movieData.movie.ImdbUrl} target="_blank" rel="noopener noreferrer">{movieData.movie.Name}</a></h4>
                <p key="movie1">
                    {movieData.movie.Actor}
                    {movieData.movie.HardheadKillCount || movieData.movie.MovieKillCount ? " (" : null}
                    {movieData.movie.HardheadKillCount ? <span>bar ábyrgð á {movieData.movie.HardheadKillCount} drápum </span> : null}
                    {movieData.movie.MovieKillCount ? "af " + movieData.movie.MovieKillCount : null}
                    {movieData.movie.HardheadKillCount || movieData.movie.MovieKillCount ? " í myndinni)" : null}
                </p>
                <p key="movie2">
                    {movieData.movie.Reason ? movieData.movie.Reason : "Gestgjafi hefur ekki skráð ástæðu fyrir mynd :("}
                </p>
                {movieData.movie.YoutubeUrl ?
                    <p key="movie3">
                        <button className="button small" onClick={toggleTrailer}>{trailerOpen ? "Fela trailer" : "Sjá trailer"}</button>
                    </p> : null}
                {trailerOpen ?
                    <YouTube key="movie4" videoId={movieData.movie.YoutubeUrl} opts={opts} /> : null}
            </section> : null
    )
}

export default Movie;