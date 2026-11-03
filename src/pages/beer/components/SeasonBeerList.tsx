import { useMemo, useState } from "react";
import { Link } from "react-router-dom";
import StarRating from "../../../components/StarRating";
import { useSeasonBeers } from "../hooks/useBeerApi";
import { BeerSummary, Season } from "../types";

interface SeasonBeerListProps {
  season: Season;
}

const styles = {
  list: { listStyle: "none", padding: 0, margin: "1em 0" },
  row: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    gap: "0.75em",
    padding: "0.6em 0",
    borderBottom: "solid 1px rgba(160, 160, 160, 0.3)",
  },
  details: { minWidth: 0, overflowWrap: "anywhere" as const },
  rating: { flexShrink: 0 },
};

// StarRating requires a handler even when read-only.
const ignoreRatingChange = () => undefined;

const normalize = (text: string) => text.trim().toLocaleLowerCase("is");

const formatAlcohol = (percentage = 0) =>
  `${percentage.toLocaleString("is-IS", { maximumFractionDigits: 1 })}%`;

const BeerRow = ({ summary }: { summary: BeerSummary }) => {
  const { beer, myRating } = summary;

  return (
    <li data-testid="beer-row" style={styles.row}>
      <div style={styles.details}>
        <Link to={`/beer/${encodeURIComponent(beer.id)}`}>
          <strong>{beer.name}</strong>
        </Link>
        <br />
        <small>
          {beer.producer} · {beer.country} ·{" "}
          {formatAlcohol(beer.alcoholPercentage)}
        </small>
      </div>
      {myRating ? (
        <span
          data-testid="my-rating"
          role="img"
          aria-label={`Þín einkunn: ${myRating} af 5`}
          style={styles.rating}
        >
          <StarRating
            rating={myRating}
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
      ) : (
        <small data-testid="not-rated" style={styles.rating}>
          Ósmakkaður
        </small>
      )}
    </li>
  );
};

const SeasonBeerList = ({ season }: SeasonBeerListProps) => {
  const { data: beers, isLoading, isError } = useSeasonBeers(season.id);
  const [search, setSearch] = useState("");

  const filtered = useMemo(() => {
    const query = normalize(search);
    return (beers ?? []).filter((s) => normalize(s.beer.name).includes(query));
  }, [beers, search]);

  if (isLoading) {
    return <p data-testid="beers-loading">Sæki bjóra…</p>;
  }

  if (isError) {
    return <p data-testid="beers-error">Ekki tókst að sækja bjórana.</p>;
  }

  return (
    <section>
      {beers && beers.length > 0 ? (
        <>
          <input
            type="search"
            aria-label="Leita að bjór"
            placeholder="Leita að bjór"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          {filtered.length > 0 ? (
            <ul style={styles.list}>
              {filtered.map((summary) => (
                <BeerRow key={summary.beer.id} summary={summary} />
              ))}
            </ul>
          ) : (
            <p data-testid="no-match">Enginn bjór fannst.</p>
          )}
        </>
      ) : (
        <p data-testid="no-beers">Engir bjórar eru skráðir á tímabilið enn.</p>
      )}
      {season.isOpen && (
        <Link
          to="/beer/register"
          className="button fit"
          data-testid="register-beer"
        >
          Skrá nýjan bjór
        </Link>
      )}
    </section>
  );
};

export default SeasonBeerList;
