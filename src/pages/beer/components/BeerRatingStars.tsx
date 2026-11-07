import { useState } from "react";
import StarRating from "../../../components/StarRating";
import { useSaveRating } from "../hooks/useBeerApi";
import { Season } from "../types";

interface BeerRatingStarsProps {
  season: Season;
  beerID: string;
  myRating?: number;
}

// What each number of stars means, shown under the stars.
export const ratingLabels: Record<number, string> = {
  1: "Vondur bjór",
  2: "Slakur bjór",
  3: "Ágætur bjór",
  4: "Góður bjór",
  5: "Frábær bjór",
};

const lineStyle = { paddingTop: "5px" };

// StarRating requires a handler even when read-only.
const ignoreRatingChange = () => undefined;

// My rating of the beer, for the post's footer, laid out like the Hardhead
// ratings: an icon, the stars, and what the hovered, focused or given stars
// mean.
const BeerRatingStars = ({
  season,
  beerID,
  myRating,
}: BeerRatingStarsProps) => {
  const saveRating = useSaveRating(season.id, beerID);
  const [hoverRating, setHoverRating] = useState(0);
  // Show the last tapped stars while saving, so the tap feels instant, and the
  // saved stars afterwards, even before the beer list has been refetched.
  const shownRating = saveRating.isPending
    ? saveRating.variables
    : saveRating.isSuccess
      ? saveRating.data.stars
      : (myRating ?? 0);
  const readonly = !season.isOpen;
  const label = ratingLabels[hoverRating || shownRating || 0];

  // A tap while an earlier save is still running is queued behind it, so the
  // user can change their mind straight away and the last tap wins.
  const handleChange = (stars: number) => saveRating.mutate(stars);

  return (
    <ul className="stats">
      <li>
        <i className="icon solid fa-beer fa-2x" aria-hidden="true" />
        <span
          data-testid="rating-stars"
          role={readonly ? "img" : "radiogroup"}
          aria-label={
            shownRating
              ? `Þín einkunn: ${shownRating} af 5`
              : "Þú hefur ekki gefið einkunn"
          }
        >
          <StarRating
            rating={shownRating ?? 0}
            starRatedColor="gold"
            starHoverColor="orange"
            starEmptyColor="rgb(226, 226, 226)"
            changeRating={readonly ? ignoreRatingChange : handleChange}
            numberOfStars={5}
            starDimension="20px"
            starSpacing="2px"
            readonly={readonly}
            onHoverChange={setHoverRating}
            asRadio
            starLabel={(stars) => ratingLabels[stars]}
          />
        </span>
        <div data-testid="rating-label" style={lineStyle}>
          {/* A non-breaking space keeps the line's height when there's no
              label, so the stars don't jump on hover (as on Hardhead). */}
          {label ?? "\u00a0"}
        </div>
        {readonly ? (
          <div style={lineStyle} data-testid="rating-closed">
            Tímabilið er búið og ekki hægt að gefa einkunn.
          </div>
        ) : saveRating.isPending ? (
          <div style={lineStyle} data-testid="rating-saving">
            Vista…
          </div>
        ) : saveRating.isError ? (
          <div style={lineStyle} data-testid="rating-error" role="alert">
            Ekki tókst að vista einkunnina. Reyndu aftur.
          </div>
        ) : saveRating.isSuccess ? (
          <div style={lineStyle} data-testid="rating-saved" role="status">
            Einkunn vistuð: {saveRating.data.stars} af 5
          </div>
        ) : null}
      </li>
    </ul>
  );
};

export default BeerRatingStars;
