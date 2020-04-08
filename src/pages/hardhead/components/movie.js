import React, {useState, useEffect} from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import YouTubeEmbed from 'react-youtube-embed';

const Movie = (propsData) => {    
    const[movieData, setMovieData] = useState({movie: null, isLoading: false, visible: false})

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
                    {/* className="row" */}
                    {/* <div className="col-6 col-12-medium"> */}
                        <a href={movieData.movie.ImdbUrl} target="_blank" rel="noopener noreferrer">{movieData.movie.Name}</a> með {movieData.movie.Actor}<br/><br/>
                        {movieData.movie.Reason ? movieData.movie.Reason : "Gestgjafi hefur ekki skráð ástæðu fyrir mynd :("}
                    {/* </div> */}
                    <br/>
                    <br/>
                    {movieData.movie.YoutubeUrl ?
                            // <div className="col-6 col-12-medium">
                                <div className="image featured">
                                    <YouTubeEmbed id={movieData.movie.YoutubeUrl}/>
                                </div>
                            // </div> 
                            :    
                            movieData.movie.PosterPhoto ?
                                // <div className="col-4">
                                    <div className="image featured">
                                        <img src={config.get("path") + movieData.movie.PosterPhoto.Href + "?code=" + config.get("code")} alt={movieData.movie.Name} />
                                    </div>
                                // </div>
                                : null                            
                        }                         
                </div> :
                null
            }
        </div>
    )
}

export default Movie;