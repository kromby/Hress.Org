import { useMemo } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { HressPage } from "../hress";
import { useSeasonBeers } from "../hooks/useBeerApi";
import { Season } from "../types";
import BeerPost from "./BeerPost";

interface SeasonBeerListProps {
  season: Season;
}

const normalize = (text = "") => text.trim().toLocaleLowerCase("is");

// The season's page: the season's post, then a post per beer and a button to
// register a new one. The nav bar search sends ?query=, which filters the
// beers by name or producer.
const SeasonBeerList = ({ season }: SeasonBeerListProps) => {
  const { data: beers, isLoading, isError } = useSeasonBeers(season.id);
  const [searchParams] = useSearchParams();
  const searchText = (searchParams.get("query") ?? "").trim();
  const query = normalize(searchText);
  const hasBeers = Boolean(beers && beers.length > 0);

  const shown = useMemo(
    () =>
      (beers ?? []).filter(
        ({ beer }) =>
          normalize(beer.name).includes(query) ||
          normalize(beer.producer).includes(query)
      ),
    [beers, query]
  );

  const status = isLoading ? (
    <p data-testid="beers-loading">Sæki bjóra…</p>
  ) : isError ? (
    <p data-testid="beers-error">Ekki tókst að sækja bjórana.</p>
  ) : !hasBeers ? (
    <p data-testid="no-beers">Engir bjórar eru skráðir á tímabilið enn.</p>
  ) : query ? (
    <p data-testid="search-status">
      {shown.length === 0 ? "Enginn bjór fannst fyrir" : "Bjórar sem passa við"}{" "}
      „{searchText}“. <Link to="/beer">Sýna alla</Link>
    </p>
  ) : null;

  const closed = !season.isOpen && (
    <p data-testid="season-closed">Þetta tímabil er búið.</p>
  );

  const below = (
    <>
      {shown.map((summary) => (
        <BeerPost key={summary.beer.id} summary={summary} />
      ))}
      {!isLoading && !isError && season.isOpen && (
        <Link
          to="/beer/register"
          className="button large fit"
          data-testid="register-beer"
        >
          Skrá nýjan bjór
        </Link>
      )}
    </>
  );

  return (
    <HressPage
      title={season.name}
      description="Bjórar tímabilsins"
      below={below}
    >
      {(closed || status) && (
        <>
          {closed}
          {status}
        </>
      )}
    </HressPage>
  );
};

export default SeasonBeerList;
