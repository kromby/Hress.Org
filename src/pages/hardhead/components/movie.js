import React, {useState, useEffect} from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import YouTube from 'react-youtube';

const Movie = (propsData) => {    
    const[movieData, setMovieData] = useState({movie: null, isLoading: false, visible: false})

    const opts = {
        width: '500',
        playerVars: {
          // https://developers.google.com/youtube/player_parameters
          autoplay: 0,
        }
    };

    var movieUrl = config.get("path") + "/api/movies/" + propsData.id + "?code=" + config.get("code");

    useEffect(() => {
        const getMovieData = async () => {
            try {
                setMovieData({isLoading: true});

                const response = await axios.get(movieUrl, {
                    //headers: {'Authorization': 'Basic ' + 'asdf'}
                })

                if(response.data !== undefined) {
                    setMovieData({movie: response.data, isLoading: false, visible: true});
                }
            }
            catch(e) {
                console.error(e);
                setMovieData({isLoading: false, visible: false});
            }
        };
        getMovieData();
    }, [propsData, movieUrl])

    return (
        <div className="col-12">
            <div className="col-12">
                <h3>Myndin</h3>
            </div>
            {movieData.visible ?
                <div>                           
                    <a href={movieData.movie.ImdbUrl} target="_blank" rel="noopener noreferrer">{movieData.movie.Name}</a> með {movieData.movie.Actor}<br/><br/>
                    {movieData.movie.Reason ? movieData.movie.Reason : "Gestgjafi hefur ekki skráð ástæðu fyrir mynd :("}
                    <br/>
                    <br/>
                    {movieData.movie.YoutubeUrl ?
                        <div className="image featured">
                            <YouTube videoId={movieData.movie.YoutubeUrl} opts={opts}/>
                        </div>
                        :    
                        movieData.movie.PosterPhoto ?
                            <div className="image featured">
                                <img src={config.get("path") + movieData.movie.PosterPhoto.Href + "?code=" + config.get("code")} alt={movieData.movie.Name} />
                            </div>
                            : null                            
                    }                         
                </div> : null
            }
        </div>
    )
}

export default Movie;