import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import YouTube from 'react-youtube';

const Movie = (propsData) => {
    const [movieData, setMovieData] = useState({ movie: null})
    const [trailerOpen, setTrailerOpen] = useState(false);

    const opts = {
        // width: '800',
        playerVars: {
            // https://developers.google.com/youtube/player_parameters
            autoplay: 0,
        }
    };

    var movieUrl = config.get("path") + "/api/movies/" + propsData.id + "?code=" + config.get("code");

    useEffect(() => {
        const getMovieData = async () => {
            try {
                const response = await axios.get(movieUrl);

                if (response.data !== undefined) {
                    setMovieData({ movie: response.data});
                    if (response.data.PosterPhoto) {
                        propsData.photoPostback(response.data.PosterPhoto.Href);
                    }
                }
            }
            catch (e) {
                if(e.response === undefined || e.response.status !== 404) {
                    console.error(e);
                }
            }
        };
        getMovieData();
    }, [propsData, movieUrl])

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