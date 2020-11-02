import React, {useState, useEffect} from 'react';
import config from 'react-global-configuration';
import axios from "axios";

const MovieEdit = (propsData) => {    
    const[data, setData] = useState({visible: false})
    const[movie, setMovie] = useState();

    var movieUrl = config.get("path") + "/api/movies/" + propsData.id + "?code=" + config.get("code");

    useEffect(() => {
        const getMovieData = async () => {
            try {
                const response = await axios.get(movieUrl);

                if(response.data !== undefined) {                    
                    setMovie(response.data);
                    setData({visible: true});
                }
            }
            catch(e) {
                console.error(e);
                setData({visible: false});
            }
        };
        getMovieData();
    }, [propsData, movieUrl])

    const handleSubmit = async (event) => {
        event.preventDefault();	
        console.log(movie);
		// if(authTokens !== undefined){			
		// 	event.preventDefault();			
		// 	try {
		// 		var url = config.get('path') + '/api/hardhead/' + propsData.match.params.hardheadID + '?code=' + config.get('code');
		// 		const response = await axios.put(url, {
		// 			ID: propsData.match.params.hardheadID,
		// 			Number: hardhead.Number,
		// 			Host: hardhead.Host,
		// 			Date: hardheadDate,
		// 			Description: description,
		// 			NextHostID: nextHostID			
		// 		}, {
		// 			headers: {'Authorization': 'token ' + authTokens.token},            
		// 		});
		// 		setData({saved: true, error: data.error, isLoaded: data.isLoaded, visible: data.visible, minDate: data.minDate, maxDate: data.maxDate});
		// 		console.log(response);
		// 	} catch(e) {
		// 		console.error(e);
		// 		alert("Ekki tókst að vista kvöld.");
		// 	}
		// } else {
		// 	// TODO: redirect to main page
		// }
    }	
    
    const handleMovieChange = (event) => { movie.Name = event.target.value; setMovie(movie);  }
    const handleActorChange = (event) => { movie.Actor = event.target.value; setMovie(movie);  }
    const handleImdbChange = (event) => { movie.ImdbUrl = event.target.value; setMovie(movie);  }
    const handleYoutubeChange = (event) => { movie.YoutubeUrl = event.target.value; setMovie(movie);  }
    const handleReasonChange = (event) => { movie.Reason = event.target.value; setMovie(movie);  }
    const handlePosterChange = (event) => { movie.PosterPhoto = {Href: event.target.value}; setMovie(movie);  }

    return (
        <form onSubmit={handleSubmit}>
            <div className="col-12">
                <div className="col-12">
                    <h3>Myndin</h3>
                </div>
                <div>                           
                    <input id="name" type="text" name="name" onChange={(ev) => handleMovieChange(ev)} defaultValue={movie ? movie.Name : null} placeholder="Nafn myndar"/>
                    <br/>
                    <input id="actor" type="text" name="actor" onChange={(ev) => handleActorChange(ev)} defaultValue={movie ? movie.Actor : null} placeholder="Harðhaus"/>
                    <br/>
                    <input id="imdb" type="text" name="imdb" onChange={(ev) => handleImdbChange(ev)} defaultValue={movie ? movie.ImdbUrl : null} placeholder="Slóð á IMDB"/>
                    <br/>
                    <input id="youtube" type="text" name="youtube"onChange={(ev) => handleYoutubeChange(ev)} defaultValue={movie ? movie.YoutubeUrl : null} placeholder="Slóð á trailer á Youtube"/>
                    <br/>
                    <textarea name="reason" rows="3" onChange={(ev) => handleReasonChange(ev)} defaultValue={movie ? movie.Reason : null} placeholder="Ástæða fyrir mynd?" />
                    <br/>
                    <input id="poster" type="text" name="poster" onChange={(ev) => handlePosterChange(ev)} placeholder="Slóð á poster"/>
                    <br/>
                    {movie ? 
                        <div className="image featured">
                            <img src={config.get("path") + movie.PosterPhoto.Href + "?code=" + config.get("code")} alt={movie.Name} />
                        </div>
                    : null}
                    <br/>
                    {/* <button tooltip="Vista mynd" className="button large">Vista mynd</button> */}
                </div>
            </div>
        </form>
    )
}

export default MovieEdit;