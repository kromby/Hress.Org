import StarRating from "../../../components/StarRating";
import { useSaveRating } from "../hooks/useBeerApi";
import { Season } from "../types";

interface BeerRatingStarsProps {
  season: Season;
  beerID: string;
  myRating?: number;
}

const styles = {
  wrapper: { margin: "1em 0" },
  message: { display: "block", marginTop: "0.5em" },
};

// StarRating requires a handler even when read-only.
const ignoreRatingChange = () => undefined;

const BeerRatingStars = ({
  season,
  beerID,
  myRating,
}: BeerRatingStarsProps) => {
  const saveRating = useSaveRating(season.id, beerID);
  // Show the tapped stars while saving, so the tap feels instant, and the
  // saved stars afterwards, even before the beer list has been refetched.
  const shownRating = saveRating.isPending
    ? saveRating.variables
    : saveRating.isSuccess
      ? saveRating.data.stars
      : (myRating ?? 0);
  const readonly = !season.isOpen;

  const handleChange = (stars: number) => {
    if (saveRating.isPending) return;
    saveRating.mutate(stars);
  };

  return (
    <div style={styles.wrapper}>
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
          asRadio
        />
      </span>
      {readonly ? (
        <small data-testid="rating-closed" style={styles.message}>
          Tímabilið er búið og ekki hægt að gefa einkunn.
        </small>
      ) : saveRating.isPending ? (
        <small data-testid="rating-saving" style={styles.message}>
          Vista…
        </small>
      ) : saveRating.isError ? (
        <small data-testid="rating-error" role="alert" style={styles.message}>
          Ekki tókst að vista einkunnina. Reyndu aftur.
        </small>
      ) : saveRating.isSuccess ? (
        <small data-testid="rating-saved" role="status" style={styles.message}>
          Einkunn vistuð: {saveRating.data.stars} af 5
        </small>
      ) : (
        !myRating && (
          <small data-testid="rating-hint" style={styles.message}>
            Smelltu á stjörnu til að gefa einkunn.
          </small>
        )
      )}
    </div>
  );
};

export default BeerRatingStars;
