import { useState, useEffect } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { useAuth } from "../../../context/auth";
import TypeAheadDropDown from "../../../components/typeAheadDropDown";
import { useLocation, useNavigate, useParams } from "react-router-dom";

const MovieEdit = ({ id }) => {
  const { authTokens } = useAuth();
  const params = useParams();
  const location = useLocation();
  const navigate = useNavigate();
  const [data, setData] = useState({ visible: false, saving: false });
  const [buttonEnabled, setButtonEnabled] = useState(false);
  const [imagePath, setImagePath] = useState("");
  const [imageGetEnabled, setImageGetEnabled] = useState(false);
  const [actor, setActor] = useState("");
  const [movieName, setMovieName] = useState("");
  const [imdbUrl, setImdbUrl] = useState("");
  const [youtubeUrl, setYoutubeUrl] = useState("");
  const [reason, setReason] = useState("");
  const [posterPhotoID, setPosterPhotoID] = useState();
  const [ID, setID] = useState();
  const [movieKills, setMovieKills] = useState();
  const [hardheadKills, setHardheadKills] = useState();
  const [omdbData, setOmdbData] = useState();

  const movieUrl =
    config.get("path") + "/api/movies/" + id + "?code=" + config.get("code");

  useEffect(() => {
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
      return;
    }

    const getMovieData = () => {
      axios
        .get(movieUrl)
        .then((response) => {
          if (response.data !== undefined) {
            if (response.data.YoutubeUrl) {
              response.data.YoutubeUrl =
                "https://www.youtube.com/watch?v=" + response.data.YoutubeUrl;
            }

            setID(response.data.ID);
            setMovieName(response.data.Name);
            setActor(response.data.Actor);
            setImdbUrl(response.data.ImdbUrl);
            setYoutubeUrl(response.data.YoutubeUrl);
            setReason(response.data.Reason);
            setImagePath(
              response.data.PosterPhoto ? response.data.PosterPhoto.Href : ""
            );
            setData({ visible: true });
            setMovieKills(response.data.MovieKillCount);
            setHardheadKills(response.data.HardheadKillCount);
          }
        })
        .catch((error) => {
          if (error.response.status === 404) {
            console.log("[MovieEdit] Movie not found");
          } else {
            console.error("[MovieEdit] Error getting movie.");
            console.log(error);
            setData({ visible: false });
          }
        });
    };

    getMovieData();
  }, [params, movieUrl]);

  const saveMovieInfo = (movieInfo) => {
    console.info("[saveMovieInfo] started");
    const url = `${config.get("apiPath")}/api/movies/${id}/info`;
    console.info("[saveMovieInfo] url", url);

    axios.put(url, movieInfo, {
      headers: { "X-Custom-Authorization": "token " + authTokens.token },
    });
    console.info("[saveMovieInfo] completed");
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setButtonEnabled(false);
    setData({ visible: data.visible, saved: false });
    if (authTokens !== undefined) {
      event.preventDefault();
      try {
        await axios.put(
          movieUrl,
          {
            ID,
            Name: movieName,
            Actor: actor,
            ImdbUrl: imdbUrl,
            YoutubeUrl: youtubeUrl,
            Reason: reason,
            PosterPhotoID: posterPhotoID,
            MovieKillCount: movieKills,
            HardheadKillCount: hardheadKills,
          },
          {
            headers: { Authorization: "token " + authTokens.token },
          }
        );

        saveMovieInfo(omdbData);

        setData({
          saved: true,
          error: data.error,
          isLoaded: data.isLoaded,
          visible: data.visible,
          minDate: data.minDate,
          maxDate: data.maxDate,
          saving: false,
        });
      } catch (e) {
        console.error("[MovieEdit] Error saving movie.");
        console.error(e);
        setData({ visible: data.visible, saving: false });
        setButtonEnabled(true);
      }
    }
  };

  const handleActorChange = (event) => {
    setActor(event.target.value);
    setButtonEnabled(true);
  };
  const handleImdbChange = (event) => {
    setImdbUrl(event.target.value);
    setButtonEnabled(true);
  };
  const handleYoutubeChange = (event) => {
    setYoutubeUrl(event.target.value);
    setButtonEnabled(true);
  };
  const handleReasonChange = (event) => {
    setReason(event.target.value);
    setButtonEnabled(true);
  };
  const handlePosterChange = (event) => {
    setImagePath(event.target.value);
    setImageGetEnabled(event.target.value.length > 6 && movieName.length > 1);
  };
  const handleMovieKillsChange = (event) => {
    setMovieKills(event.target.value);
    setButtonEnabled(true);
  };
  const handleHardhaedKillsChanges = (event) => {
    setHardheadKills(event.target.value);
    setButtonEnabled(true);
  };

  const getPostImage = async (event, path) => {
    if (event) {
      event.preventDefault();
    }
    setImageGetEnabled(false);

    const useImagePath = path ? path : imagePath;

    if (authTokens !== undefined) {
      try {
        const response = await axios.post(
          config.get("imagePath") + "/api/Images",
          {
            source: useImagePath,
            name: movieName,
          }
        );
        if (response.status === 201) {
          if (response.data) {
            setPosterPhotoID(response.data.id);
            setImagePath("/api/Images/" + response.data.id + "/content");
            setButtonEnabled(true);
          }
        }
      } catch (e) {
        console.error("[MovieEdit] Error saving new movie poster");
        console.error(e);
        setImageGetEnabled(true);
      }
    }
  };

  const movieCallback = async (imdbId) => {
    try {
      const response = await axios.get(
        "https://www.omdbapi.com/?apikey=" + config.get("omdb") + "&i=" + imdbId
      );

      setMovieName(response.data.Title);
      setActor(response.data.Actors.split(",")[0]);
      setImdbUrl("https://www.imdb.com/title/" + imdbId);
      if (response.data.Poster !== "N/A") {
        setImagePath(response.data.Poster);
        getPostImage(null, response.data.Poster);
      }
      setImageGetEnabled(true);
      setButtonEnabled(true);
      setOmdbData(response.data);
    } catch (e) {
      console.error("[MovieEdit] Error getting data from omdbapi");
      console.error(e);
    }
  };

  return (
    <section>
      <h3>Myndin</h3>
      {imagePath ? (
        <span className="image right">
          {imagePath.startsWith("http") ? (
            <img src={imagePath} alt={movieName} />
          ) : (
            <img src={config.get("apiPath") + imagePath} alt={movieName} />
          )}
        </span>
      ) : null}
      <form onSubmit={handleSubmit}>
        <div className="row gtr-uniform">
          <div className="col-6 col-12-xsmall">
            <TypeAheadDropDown
              defaultValue={movieName}
              placeholder="Nafn myndar"
              callback={(newId) => movieCallback(newId)}
            />
          </div>
          <div className="col-6 col-12-xsmall">
            <input
              id="actor"
              type="text"
              name="actor"
              onChange={(ev) => handleActorChange(ev)}
              defaultValue={actor}
              placeholder="Harðhaus"
            />
          </div>
          <div className="col-6 col-12-xsmall">
            <input
              id="movieKills"
              type="text"
              name="movieKills"
              onChange={(ev) => handleMovieKillsChange(ev)}
              defaultValue={movieKills}
              placeholder="Heildardráp"
            />
          </div>
          <div className="col-6 col-12-xsmall">
            <input
              id="hardheadKills"
              type="text"
              name="hardheadKills"
              onChange={(ev) => handleHardhaedKillsChanges(ev)}
              defaultValue={hardheadKills}
              placeholder="Harðhausadráp"
            />
          </div>
          <div className="col-12">
            <input
              id="imdb"
              type="text"
              name="imdb"
              onChange={(ev) => handleImdbChange(ev)}
              defaultValue={imdbUrl}
              placeholder="Slóð á IMDB"
            />
          </div>
          <div className="col-12">
            <input
              id="youtube"
              type="text"
              name="youtube"
              onChange={(ev) => handleYoutubeChange(ev)}
              defaultValue={youtubeUrl}
              placeholder="Slóð á trailer á Youtube"
            />
          </div>
          <div className="col-12">
            <textarea
              name="reason"
              rows="3"
              onChange={(ev) => handleReasonChange(ev)}
              defaultValue={reason}
              placeholder="Ástæða fyrir mynd?"
            />
          </div>
          <div className="col-8 col-12-xsmall">
            <input
              id="poster"
              type="text"
              name="poster"
              onChange={(ev) => handlePosterChange(ev)}
              defaultValue={imagePath}
              placeholder="Slóð á poster"
            />
          </div>
          <div className="col-4 col-12-xsmall">
            <button
              className="button small"
              disabled={!imageGetEnabled}
              onClick={(ev) => getPostImage(ev)}
            >
              Sækja mynd
            </button>
          </div>
          <div className="col-12">
            {data.saved ? (
              <b>
                Mynd vistuð!
                <br />
              </b>
            ) : null}
            <button className="button large" disabled={!buttonEnabled}>
              Vista mynd
            </button>
          </div>
        </div>
      </form>
    </section>
  );
};

export default MovieEdit;
