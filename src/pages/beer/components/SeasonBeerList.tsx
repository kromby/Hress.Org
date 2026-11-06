import { Link } from "react-router-dom";
import { HressPage } from "../hress";
import { useSeasonBeers } from "../hooks/useBeerApi";
import { Season } from "../types";
import BeerPost from "./BeerPost";

interface SeasonBeerListProps {
  season: Season;
}

// The season's page: the season's post, then a post per beer and a button to
// register a new one.
const SeasonBeerList = ({ season }: SeasonBeerListProps) => {
  const { data: beers, isLoading, isError } = useSeasonBeers(season.id);
  const hasBeers = Boolean(beers && beers.length > 0);

  const status = isLoading ? (
    <p data-testid="beers-loading">Sæki bjóra…</p>
  ) : isError ? (
    <p data-testid="beers-error">Ekki tókst að sækja bjórana.</p>
  ) : !hasBeers ? (
    <p data-testid="no-beers">Engir bjórar eru skráðir á tímabilið enn.</p>
  ) : null;

  const closed = !season.isOpen && (
    <p data-testid="season-closed">Þetta tímabil er búið.</p>
  );

  const below = (
    <>
      {beers?.map((summary) => (
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
