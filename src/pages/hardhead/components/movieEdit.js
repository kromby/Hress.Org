import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import { useAuth } from '../../../context/auth';

const MovieEdit = (propsData) => {
    const { authTokens } = useAuth();
    const [data, setData] = useState({ visible: false, saving: false })
    const [movie, setMovie] = useState();
    const [buttonEnabled, setButtonEnabled] = useState(false);
    const [imagePath, setImagePath] = useState();
    const [imageGetEnabled, setImageGetEnabled] = useState(false);

    var movieUrl = config.get("path") + "/api/movies/" + propsData.id + "?code=" + config.get("code");

    useEffect(() => {
        const getMovieData = async () => {
            try {
                const response = await axios.get(movieUrl);

                if (response.data !== undefined) {
                    if (response.data.YoutubeUrl) {
                        response.data.YoutubeUrl = "https://www.youtube.com/watch?v=" + response.data.YoutubeUrl;
                    }

                    setMovie(response.data);
                    setData({ visible: true });
                }
            }
            catch (e) {
                console.error("Error getting movie.");
                console.error(e);
                setMovie({});
                setData({ visible: false });
            }
        };
        getMovieData();
    }, [propsData, movieUrl])

    const handleSubmit = async (event) => {
        event.preventDefault();
        setButtonEnabled(false);
        setData({ visible: data.visible, saved: false });
        console.log(movie);
        if (authTokens !== undefined) {
            event.preventDefault();
            try {
                const response = await axios.put(movieUrl, movie, {
                    headers: { 'Authorization': 'token ' + authTokens.token },
                });
                setData({ saved: true, error: data.error, isLoaded: data.isLoaded, visible: data.visible, minDate: data.minDate, maxDate: data.maxDate, saving: false });

                console.log(response);
            } catch (e) {
                console.error(e);
                alert("Ekki tókst að vista kvöld.");
                setData({ visible: data.visible, saving: false });
                setButtonEnabled(true);
            }
        } else {
            // TODO: redirect to main page
        }

    }

    const handleMovieChange = (event) => { movie.Name = event.target.value; setMovie(movie); setButtonEnabled(true); }
    const handleActorChange = (event) => { movie.Actor = event.target.value; setMovie(movie); setButtonEnabled(true); }
    const handleImdbChange = (event) => { movie.ImdbUrl = event.target.value; setMovie(movie); setButtonEnabled(true); }
    const handleYoutubeChange = (event) => { movie.YoutubeUrl = event.target.value; setMovie(movie); setButtonEnabled(true); }
    const handleReasonChange = (event) => { movie.Reason = event.target.value; setMovie(movie); setButtonEnabled(true); }
    const handlePosterChange = (event) => { setImagePath(event.target.value); setImageGetEnabled(event.target.value.length > 6 && movie.Name.length > 1); }

    const getPostImage = async (event) => {
        event.preventDefault();
        setImageGetEnabled(false);

        if (authTokens !== undefined) {
            try {
                const response = await axios.post(config.get('imagePath') + '/api/Images', {
                    source: imagePath,
                    name: movie.Name
                }, {
                    // headers: { 'Authorization': 'token ' + authTokens.token },
                });
                console.log(response);
                if (response.status === 201) {
                    console.log(response.data);
                    if (response.data) {
                        movie.PosterPhotoID = response.data.id;
                        setMovie(movie);
                        setButtonEnabled(true);
                    }
                }
            } catch (e) {
                console.error(e);
                alert("Ekki tókst að sækja mynd.");
                setImageGetEnabled(true);
            }
        }
    }

    return (
        <section>
            <h3>Myndin</h3>
            {movie && movie.PosterPhoto ?
                <span className="image right">
                    <img src={config.get("imagePath") + movie.PosterPhoto.Href + "?code=" + config.get("code")} alt={movie.Name} />
                </span>
                : null}
            <form onSubmit={handleSubmit}>
                <div className="row gtr-uniform">
                    <div className="col-6 col-12-xsmall">
                        <input id="name" type="text" name="name" onChange={(ev) => handleMovieChange(ev)} defaultValue={movie ? movie.Name : null} placeholder="Nafn myndar" />
                    </div>
                    <div className="col-6 col-12-xsmall">
                        <input id="actor" type="text" name="actor" onChange={(ev) => handleActorChange(ev)} defaultValue={movie ? movie.Actor : null} placeholder="Harðhaus" />
                    </div>
                    <div className="col-12">
                        <input id="imdb" type="text" name="imdb" onChange={(ev) => handleImdbChange(ev)} defaultValue={movie ? movie.ImdbUrl : null} placeholder="Slóð á IMDB" />
                    </div>
                    <div className="col-12">
                        <input id="youtube" type="text" name="youtube" onChange={(ev) => handleYoutubeChange(ev)} defaultValue={movie ? movie.YoutubeUrl : null} placeholder="Slóð á trailer á Youtube" />
                    </div>
                    <div className="col-12">
                        <textarea name="reason" rows="3" onChange={(ev) => handleReasonChange(ev)} defaultValue={movie ? movie.Reason : null} placeholder="Ástæða fyrir mynd?" />
                    </div>
                    <div className="col-8 col-12-xsmall">
                        <input id="poster" type="text" name="poster" onChange={(ev) => handlePosterChange(ev)} defaultValue={movie && movie.PosterPhoto ? movie.PosterPhoto.Href : null} placeholder="Slóð á poster" />
                    </div>
                    <div className="col-4 col-12-xsmall">
                        <button tooltip="Sækja mynd" className="button small" disabled={!imageGetEnabled} onClick={(ev) => getPostImage(ev)}>Sækja mynd</button>
                    </div>
                    <div className="col-12">
                        {data.saved ? <b>Kvöld vistað!<br /></b> : null}
                        <button tooltip="Vista mynd" className="button large" disabled={!buttonEnabled}>Vista mynd</button>
                    </div>
                </div>
            </form>
        </section>
    )
}

export default MovieEdit;