import { Link } from "react-router-dom";
import { CatalogBeer } from "../types";
import BeerCover from "./BeerCover";
import { formatAlcohol } from "./BeerPost";

interface BeerMatchesProps {
  matches: CatalogBeer[];
  onAdd: (beerID: string) => void;
  // Disables every "Bæta við" while an add is in flight.
  adding: boolean;
}

const rowStyle = {
  display: "flex",
  alignItems: "center",
  gap: "1em",
  padding: "0.5em 0",
};

// Catalog beers that match the name being typed. Each can be added to the
// season, or opened if it's already there.
const BeerMatches = ({ matches, onAdd, adding }: BeerMatchesProps) => (
  <ul
    data-testid="beer-matches"
    style={{ listStyle: "none", margin: 0, padding: 0 }}
  >
    {matches.map(({ beer, latestSeasonName, inCurrentSeason }) => (
      <li key={beer.id} style={rowStyle}>
        <BeerCover beer={beer} />
        <div style={{ flex: 1, minWidth: 0, overflowWrap: "anywhere" }}>
          <strong>{beer.name}</strong>
          <br />
          {beer.producer} · {formatAlcohol(beer.alcoholPercentage)}
          {latestSeasonName && (
            <>
              <br />
              <small>{latestSeasonName}</small>
            </>
          )}
        </div>
        {inCurrentSeason ? (
          <Link
            to={`/beer/${encodeURIComponent(beer.id)}`}
            aria-label={`${beer.name} er á tímabilinu`}
          >
            er á tímabilinu
          </Link>
        ) : (
          <button
            type="button"
            className="button small"
            disabled={adding}
            aria-label={`Bæta við ${beer.name}`}
            onClick={() => onAdd(beer.id)}
          >
            Bæta við
          </button>
        )}
      </li>
    ))}
  </ul>
);

export default BeerMatches;
