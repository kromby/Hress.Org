import { Link } from "react-router-dom";
import StarRating from "../../../components/StarRating";
import { HressPost } from "../hress";
import { BeerSummary } from "../types";

// StarRating requires a handler even when read-only.
const ignoreRatingChange = () => undefined;

const formatAlcohol = (percentage = 0) =>
  `${percentage.toLocaleString("is-IS", { maximumFractionDigits: 1 })}%`;

const MyStars = ({ rating }: { rating: number }) => (
  <span
    data-testid="my-rating"
    role="img"
    aria-label={`Þín einkunn: ${rating} af 5`}
  >
    <StarRating
      rating={rating}
      starRatedColor="gold"
      starHoverColor="orange"
      starEmptyColor="rgb(226, 226, 226)"
      changeRating={ignoreRatingChange}
      numberOfStars={5}
      starDimension="18px"
      starSpacing="1px"
      readonly
    />
  </span>
);

interface RatePromptProps {
  beerName: string;
  beerPath: string;
}

const RatePrompt = ({ beerName, beerPath }: RatePromptProps) => (
  <>
    <span className="published" data-testid="not-rated">
      Ósmakaður
    </span>
    {/* Wrapped: the theme resets padding on the meta column's children. */}
    <span>
      <Link
        to={beerPath}
        className="button small"
        data-testid="rate-beer"
        aria-label={`Gefa ${beerName} einkunn`}
      >
        Gefa einkunn
      </Link>
    </span>
  </>
);

interface BeerPostProps {
  summary: BeerSummary;
}

// A beer in the season's list, as a hress.org post: name, producer and
// strength, with my rating or a prompt to rate it in the meta column.
const BeerPost = ({ summary }: BeerPostProps) => {
  const { beer, myRating } = summary;
  const beerPath = `/beer/${encodeURIComponent(beer.id)}`;

  return (
    <HressPost
      title={beer.name}
      href={beerPath}
      description={`${beer.producer} · ${formatAlcohol(beer.alcoholPercentage)}`}
      meta={
        myRating ? (
          <MyStars rating={myRating} />
        ) : (
          <RatePrompt beerName={beer.name} beerPath={beerPath} />
        )
      }
    />
  );
};

export default BeerPost;
