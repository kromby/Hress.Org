import { useEffect, useState } from "react";
import config from "react-global-configuration";
import { useAuth } from "../../../context/auth";
import axios from "axios";
import { useRatings } from "../hooks/useRatings";
import { RatingEntity } from "../../../types/ratings";
import StarRating from "../../../components/StarRating";

interface HardheadRatingProps {
  id: number;
  nightRatingVisible: boolean;
  movieRatingVisible: boolean;
}

const HardheadRating = ({
  id,
  nightRatingVisible,
  movieRatingVisible,
}: HardheadRatingProps) => {
  const { authTokens } = useAuth();
  const { ratings, refreshRatings } = useRatings(id);
  const [hoverRatings, setHoverRatings] = useState<{ [key: string]: number }>(
    {}
  );

  const getRatingText = (rate: number | undefined, type: string) => {
    if (!rate) return "";
    rate = Math.round(rate);
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

        refreshRatings();
      } catch (e) {
        console.error(e);
        alert("Ekki tókst að vista einkunn, reyndu aftur síðar.");
      }
    }
  };

  const handleHoverChange = (rating: number, code: string) => {
    setHoverRatings((prev) => ({
      ...prev,
      [code]: rating,
    }));
  };

  return (
    <ul className="stats">
      {authTokens && authTokens ? null : (
        <li>Skráðu þig inn til þess að gefa einkunn</li>
      )}
      {ratings?.ratings?.map((rating: RatingEntity) =>
        (rating.code === "REP_C_RTNG" && nightRatingVisible === true) ||
        (rating.code === "REP_C_MRTNG" && movieRatingVisible === true) ? (
          <li key={rating.code}>
            <span id={`${rating.code}_${id}`} />
            {rating.code === "REP_C_RTNG" ? (
              <i className="icon solid fa-beer fa-2x" />
            ) : (
              <i className="icon solid fa-film fa-2x" />
            )}
            {ratings.readonly &&
            rating.myRating === undefined &&
            rating.averageRating === undefined ? null : (
              <>
                <StarRating
                  rating={
                    ratings.readonly
                      ? rating.averageRating ?? 0
                      : rating.myRating ?? 0
                  }
                  starRatedColor="gold"
                  starHoverColor="orange"
                  starEmptyColor="rgb(226, 226, 226)"
                  changeRating={(newRating: number) =>
                    saveRating(newRating, rating.code)
                  }
                  numberOfStars={5}
                  starDimension="20px"
                  starSpacing="2px"
                  readonly={ratings.readonly}
                  onHoverChange={(hoverRating) =>
                    handleHoverChange(hoverRating, rating.code)
                  }
                />
                <div style={{ paddingTop: "5px" }}>
                  {getRatingText(
                    hoverRatings[rating.code] ||
                      (ratings.readonly
                        ? rating.averageRating
                        : rating.myRating),
                    rating.code
                  )}
                  {ratings.readonly && rating.averageRating
                    ? `(${Math.round(rating.averageRating * 10) / 10})`
                    : null}
                </div>
              </>
            )}
            {ratings.readonly ? (
              <div style={{ paddingTop: "5px" }}>
                ({rating.numberOfRatings ?? 0} atkvæði
                {rating.myRating ? ` -  Þitt ${rating.myRating}` : null})
              </div>
            ) : null}
          </li>
        ) : null
      )}
    </ul>
  );
};

export default HardheadRating;
