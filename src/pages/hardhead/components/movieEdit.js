import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import { useAuth } from '../../../context/auth';
import TypeAheadDropDown from '../../../components/typeAheadDropDown';

const MovieEdit = (propsData) => {
    const { authTokens } = useAuth();
    const [data, setData] = useState({ visible: false, saving: false })
    //const [movie, setMovie] = useState();
    const [buttonEnabled, setButtonEnabled] = useState(false);
    const [imagePath, setImagePath] = useState("");
    const [imageGetEnabled, setImageGetEnabled] = useState(false);
    const [omdbData, setOmdbData] = useState();
    const [actor, setActor] = useState("");
    const [movieName, setMovieName] = useState("");
    const [imdbUrl, setImdbUrl] = useState("");
    const [youtubeUrl, setYoutubeUrl] = useState("");
    const [reason, setReason] = useState("");
    const [posterPhotoID, setPosterPhotoID] = useState();
    const [ID, setID] = useState();

    var movieUrl = config.get("path") + "/api/movies/" + propsData.id + "?code=" + config.get("code");

    useEffect(() => {
        const getMovieData = async () => {
            try {
                const response = await axios.get(movieUrl);

                if (response.data !== undefined) {
                    if (response.data.YoutubeUrl) {
                        response.data.YoutubeUrl = "https://www.youtube.com/watch?v=" + response.data.YoutubeUrl;
                    }

                    //setMovie(response.data);
                    setID(response.data.ID);
                    setMovieName(response.data.Name);
                    setActor(response.data.Actor);
                    setImdbUrl(response.data.ImdbUrl);
                    setYoutubeUrl(response.data.YoutubeUrl);
                    setReason(response.data.Reason);
                    setImagePath(response.data.PosterPhoto ? response.data.PosterPhoto.Href : "");
                    setData({ visible: true });
                }
            }
            catch (e) {
                console.error("Error getting movie.");
                console.error(e);
                //setMovie({});
                setData({ visible: false });
            }
        };
        getMovieData();
    }, [propsData, movieUrl])

    const handleSubmit = async (event) => {
        event.preventDefault();
        setButtonEnabled(false);
        setData({ visible: data.visible, saved: false });
        //console.log(movie);
        if (authTokens !== undefined) {
            event.preventDefault();
            try {
                const response = await axios.put(movieUrl, {
                    ID: ID,
                    Name: movieName,
                    Actor: actor,
                    ImdbUrl: imdbUrl,
                    YoutubeUrl: youtubeUrl,
                    Reason: reason,
                    PosterPhotoID: posterPhotoID
                }, {
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

    const handleActorChange = (event) => { console.log(event.target.value); setActor(event.target.value); setButtonEnabled(true); }
    const handleImdbChange = (event) => { setImdbUrl(event.target.value); setButtonEnabled(true); }
    const handleYoutubeChange = (event) => { setYoutubeUrl(event.target.value); setButtonEnabled(true); }
    const handleReasonChange = (event) => { setReason(event.target.value); setButtonEnabled(true); }
    const handlePosterChange = (event) => { setImagePath(event.target.value); setImageGetEnabled(event.target.value.length > 6 && movieName.length > 1); }

    const movieCallback = async (imdbId) => {
        try {
            const response = await axios.get("http://www.omdbapi.com/?apikey=" + config.get("omdb") + "&i=" + imdbId);
            console.log(response);

            setMovieName(response.data.Title);
            setActor(response.data.Actors.split(',')[0]);
            setImdbUrl("https://www.imdb.com/title/" + imdbId);
            if (response.data.Poster !== "N/A") {
                setImagePath(response.data.Poster);
                getPostImage(null, response.data.Poster);
            }
            setImageGetEnabled(true);
            setButtonEnabled(true);
            setOmdbData(response.data);            
        } catch (e) {
            console.error(e);
        }
    }

    const getPostImage = async (event, path) => {
        if (event) {
            event.preventDefault();
        }
        setImageGetEnabled(false);

        const useImagePath = path ? path : imagePath;

        if (authTokens !== undefined) {
            try {
                const response = await axios.post(config.get('imagePath') + '/api/Images', {
                    source: useImagePath,
                    name: movieName
                }, {
                    // headers: { 'Authorization': 'token ' + authTokens.token },
                });
                console.log(response);
                if (response.status === 201) {
                    console.log(response.data);
                    if (response.data) {
                        setPosterPhotoID(response.data.id)
                        setImagePath("/api/Images/" + response.data.id + "/content");
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
            {imagePath ?
                <span className="image right">
                    {imagePath.startsWith("http") ?
                        <img src={imagePath} alt={movieName} /> :
                        <img src={config.get("imagePath") + imagePath + "?code=" + config.get("code")} alt={movieName} />
                    }
                </span>
                : null}
            <form onSubmit={handleSubmit}>
                <div className="row gtr-uniform">
                    {/* <div className="col-6 col-12-xsmall">
                        <input id="name" type="text" name="name" onChange={(ev) => handleMovieChange(ev)} defaultValue={movie ? movie.Name : null} placeholder="Nafn myndar" />
                        <ul>
                            {omdbData && omdbData.Search ? 
                            omdbData.Search.map(m => 
                            <li key={m.imdbID}>
                                {m.Title} ({m.Year})
                                <img src={m.Poster} alt={m.Title} />
                                </li>)
                             : null}
                        </ul>
                    </div> */}
                    <div className="col-6 col-12-xsmall">
                        <TypeAheadDropDown defaultValue={movieName} placeholder="Nafn myndar" callback={(id) => movieCallback(id)} />
                    </div>
                    <div className="col-6 col-12-xsmall">
                        <input id="actor" type="text" name="actor" onChange={(ev) => handleActorChange(ev)} value={actor} placeholder="Harðhaus" />
                    </div>
                    <div className="col-12">
                        <input id="imdb" type="text" name="imdb" onChange={(ev) => handleImdbChange(ev)} value={imdbUrl} placeholder="Slóð á IMDB" />
                    </div>
                    <div className="col-12">
                        <input id="youtube" type="text" name="youtube" onChange={(ev) => handleYoutubeChange(ev)} defaultValue={youtubeUrl} placeholder="Slóð á trailer á Youtube" />
                    </div>
                    <div className="col-12">
                        <textarea name="reason" rows="3" onChange={(ev) => handleReasonChange(ev)} defaultValue={reason} placeholder="Ástæða fyrir mynd?" />
                    </div>
                    <div className="col-8 col-12-xsmall">
                        <input id="poster" type="text" name="poster" onChange={(ev) => handlePosterChange(ev)} value={imagePath} placeholder="Slóð á poster" />
                    </div>
                    <div className="col-4 col-12-xsmall">
                        <button tooltip="Sækja mynd" className="button small" disabled={!imageGetEnabled} onClick={(ev) => getPostImage(ev, undefined)}>Sækja mynd</button>
                    </div>
                    <div className="col-12">
                        {data.saved ? <b>Mynd vistuð!<br /></b> : null}
                        <button tooltip="Vista mynd" className="button large" disabled={!buttonEnabled}>Vista mynd</button>
                    </div>
                </div>
            </form>
        </section>
    )
}

export default MovieEdit;