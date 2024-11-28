import { useEffect, useState } from "react";
import config from "react-global-configuration";
import { useAuth } from "../../../context/auth";
import axios from "axios";
import StarRatings from "react-star-ratings";

const HardheadRating = ({
  id,
  nightRatingVisible,
  movieRatingVisible,
}: {
  id: number;
  nightRatingVisible: boolean;
  movieRatingVisible: boolean;
}) => {
  const { authTokens } = useAuth();
  const [data, setData] = useState<{
    ratings: any;
    isLoading: boolean;
    visible: boolean;
  }>({ ratings: [], isLoading: false, visible: true });
  const [lastLoggedIn, setLastLoggedIn] = useState(false);

  const getRatingData = async () => {
    if (authTokens !== undefined) {
      try {
        const url = `${config.get(
          "path"
        )}/api/hardhead/${id}/ratings?code=${config.get("code")}`;
        const response = await axios.get(url, {
          headers: { Authorization: `token ${authTokens.token}` },
        });
        setData({ ratings: response.data, isLoading: false, visible: true });
      } catch (e) {
        console.error(e);
        setData({ ratings: [], isLoading: false, visible: false });
      }
    }
  };

  useEffect(() => {
    const loggedIn = authTokens ? true : false;

    if (!data.ratings || lastLoggedIn !== loggedIn) {
      getRatingData();
      setLastLoggedIn(loggedIn);
    }
  }, [id, authTokens]);

  const getRatingText = (rate: number, type: string) => {
    if (rate === 1)
      return type === "REP_C_RTNG" ? "dræmt kvöld " : "hræðileg mynd ";
    else if (rate === 2)
      return type === "REP_C_RTNG" ? "ágætt kvöld " : "slæm mynd ";
    else if (rate === 3)
      return type === "REP_C_RTNG" ? "gott kvöld " : "ágæt mynd ";
    else if (rate === 4)
      return type === "REP_C_RTNG" ? "hresst kvöld " : "góð mynd ";
    else if (rate === 5)
      return type === "REP_C_RTNG" ? "frábært kvöld " : "frábær mynd ";
    else return "";
  };

  const saveRating = async (rate: number, type: string) => {
    if (authTokens !== undefined) {
      try {
        const url = `${config.get(
          "path"
        )}/api/hardhead/${id}/ratings?code=${config.get("code")}`;
        await axios.post(
          url,
          {
            type,
            rating: rate,
          },
          {
            headers: { Authorization: `token ${authTokens.token}` },
          }
        );

        getRatingData();
      } catch (e) {
        console.error(e);
        alert("Ekki tókst að vista einkunn, reyndu aftur síðar.");
      }
    }
  };

  return (
    <ul className="stats">
      {authTokens && data.visible ? null : (
        <li>Skráðu þig inn til þess að gefa einkunn</li>
      )}
      {data.ratings !== undefined && data.ratings.Ratings
        ? data.ratings.Ratings.map((rating: any) =>
            (rating.Code === "REP_C_RTNG" && nightRatingVisible === true) ||
            (rating.Code === "REP_C_MRTNG" && movieRatingVisible === true) ? (
              <li key={rating.Code}>
                <span id={`${rating.Code}_${id}`} />
                {rating.Code === "REP_C_RTNG" ? (
                  <i className="icon solid fa-beer fa-2x" />
                ) : (
                  <i className="icon solid fa-film fa-2x" />
                )}
                {data.ratings.Readonly &&
                rating.MyRating === undefined &&
                rating.AverageRating === undefined ? null : (
                  <>
                    <StarRatings
                      rating={
                        data.ratings.Readonly
                          ? rating.AverageRating
                          : rating.MyRating
                      }
                      starRatedColor="gold"
                      starHoverColor="orange"
                      starEmptyColor="rgb(203, 211, 227)"
                      changeRating={(newRating: number) =>
                        saveRating(newRating, rating.Code)
                      }
                      numberOfStars={5}
                      name={`rating_${rating.Code}`}
                      starDimension="20px"
                      starSpacing="2px"
                    />
                    <div style={{ paddingTop: "5px" }}>
                      {getRatingText(
                        data.ratings.Readonly
                          ? rating.AverageRating
                          : rating.MyRating,
                        rating.Code
                      )}
                    </div>
                  </>
                )}
                {data.ratings.Readonly ? (
                  <div style={{ paddingTop: "5px" }}>
                    (Fjöldi: {rating.NumberOfRatings}
                    {rating.MyRating ? ` -  Þú: ${rating.MyRating}` : null}
                    )&nbsp;
                  </div>
                ) : null}
              </li>
            ) : null
          )
        : null}
    </ul>
  );
};

export default HardheadRating;
