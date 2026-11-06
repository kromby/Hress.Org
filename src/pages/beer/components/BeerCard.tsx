import { Link } from "react-router-dom";
import StarRating from "../../../components/StarRating";
import { Beer, BeerSummary } from "../types";

const border = "solid 1px rgba(160, 160, 160, 0.3)";

export const cardStyle = {
  background: "#ffffff",
  border,
  margin: "0 0 1.5em 0",
};

const styles = {
  card: { ...cardStyle, display: "flex", alignItems: "stretch" },
  main: {
    display: "flex",
    alignItems: "center",
    gap: "clamp(0.6em, 2.5vw, 1em)",
    flex: 1,
    minWidth: 0,
    padding: "clamp(0.75em, 3.5vw, 1.25em)",
  },
  // Placeholder until beers have a main image (BR-34).
  image: {
    flexShrink: 0,
    width: "36px",
    height: "64px",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    border,
    borderRadius: "4px",
    background: "#f4f4f2",
    color: "#aaa",
    fontSize: "8px",
    letterSpacing: "0.1em",
  },
  header: { minWidth: 0 },
  // Smaller and tighter than the theme's post titles, and smaller still on
  // narrow phones, so names fit.
  name: {
    fontSize: "clamp(0.75em, 3.6vw, 0.9em)",
    letterSpacing: "0.18em",
    lineHeight: 1.4,
    margin: 0,
    overflowWrap: "break-word" as const,
  },
  details: {
    fontSize: "0.5em",
    letterSpacing: "0.15em",
    lineHeight: 1.6,
    margin: "0.5em 0 0 0",
  },
  side: {
    flexShrink: 0,
    width: "clamp(6em, 26vw, 7em)",
    borderLeft: border,
    padding: "1.25em clamp(0.5em, 2vw, 0.75em)",
    display: "flex",
    flexDirection: "column" as const,
    alignItems: "center",
    justifyContent: "center",
    gap: "0.6em",
    textAlign: "center" as const,
  },
  notRated: {
    fontFamily: '"Raleway", Helvetica, sans-serif',
    fontSize: "0.6em",
    letterSpacing: "0.25em",
    textTransform: "uppercase" as const,
  },
  // The theme's buttons don't wrap; let the label take two lines in the narrow column.
  rateButton: {
    margin: 0,
    maxWidth: "100%",
    height: "auto",
    padding: "0.75em 1.5em",
    lineHeight: 1.8,
    whiteSpace: "normal" as const,
  },
};

// StarRating requires a handler even when read-only.
const ignoreRatingChange = () => undefined;

const formatAlcohol = (percentage = 0) =>
  `${percentage.toLocaleString("is-IS", { maximumFractionDigits: 1 })}%`;

interface BeerHeadingProps {
  beer: Beer;
  beerPath: string;
}

const BeerHeading = ({ beer, beerPath }: BeerHeadingProps) => (
  <header style={styles.header}>
    <h2 style={styles.name}>
      <Link to={beerPath}>{beer.name}</Link>
    </h2>
    <p style={styles.details}>
      {beer.producer} · {formatAlcohol(beer.alcoholPercentage)}
    </p>
  </header>
);

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
      starDimension="14px"
      starSpacing="1px"
      readonly
    />
  </span>
);

const RatePrompt = ({ beer, beerPath }: BeerHeadingProps) => (
  <>
    <span data-testid="not-rated" style={styles.notRated}>
      Ósmakaður
    </span>
    <Link
      to={beerPath}
      className="button small"
      style={styles.rateButton}
      data-testid="rate-beer"
      aria-label={`Gefa ${beer.name} einkunn`}
    >
      Gefa einkunn
    </Link>
  </>
);

interface BeerCardProps {
  summary: BeerSummary;
}

// A beer in the season's list: image, name and details, with my rating or a
// prompt to rate it.
const BeerCard = ({ summary }: BeerCardProps) => {
  const { beer, myRating } = summary;
  const beerPath = `/beer/${encodeURIComponent(beer.id)}`;

  return (
    <li data-testid="beer-row" style={styles.card}>
      <div style={styles.main}>
        <div style={styles.image} aria-hidden="true">
          MYND
        </div>
        <BeerHeading beer={beer} beerPath={beerPath} />
      </div>
      <div style={styles.side}>
        {myRating ? (
          <MyStars rating={myRating} />
        ) : (
          <RatePrompt beer={beer} beerPath={beerPath} />
        )}
      </div>
    </li>
  );
};

export default BeerCard;
