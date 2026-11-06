import { useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { HressPage } from "../hress";
import { useSeasonBeers } from "../hooks/useBeerApi";
import { Season } from "../types";
import BeerCard, { cardStyle } from "./BeerCard";

interface SeasonBeerListProps {
  season: Season;
}

const styles = {
  list: { listStyle: "none", padding: 0, margin: 0 },
  registerCard: { ...cardStyle, padding: "1.5em" },
  register: { margin: 0 },
};

const normalize = (text: string) => text.trim().toLocaleLowerCase("is");

// The season's page: the season's post card with search and messages, and a
// card per beer below it on the page background.
const SeasonBeerList = ({ season }: SeasonBeerListProps) => {
  const { data: beers, isLoading, isError } = useSeasonBeers(season.id);
  const [search, setSearch] = useState("");

  const filtered = useMemo(() => {
    const query = normalize(search);
    return (beers ?? []).filter((s) => normalize(s.beer.name).includes(query));
  }, [beers, search]);

  const hasBeers = Boolean(beers && beers.length > 0);

  const below = (
    <>
      {hasBeers && filtered.length > 0 && (
        <ul style={styles.list} aria-label="Bjórar tímabilsins">
          {filtered.map((summary) => (
            <BeerCard key={summary.beer.id} summary={summary} />
          ))}
        </ul>
      )}
      {!isLoading && !isError && season.isOpen && (
        <div style={styles.registerCard}>
          <Link
            to="/beer/register"
            className="button fit"
            style={styles.register}
            data-testid="register-beer"
          >
            Skrá nýjan bjór
          </Link>
        </div>
      )}
    </>
  );

  return (
    <HressPage
      title={season.name}
      description="Bjórar tímabilsins"
      below={below}
    >
      {!season.isOpen && (
        <p data-testid="season-closed">Þetta tímabil er búið.</p>
      )}
      {isLoading ? (
        <p data-testid="beers-loading">Sæki bjóra…</p>
      ) : isError ? (
        <p data-testid="beers-error">Ekki tókst að sækja bjórana.</p>
      ) : hasBeers ? (
        <>
          <input
            type="search"
            aria-label="Leita að bjór"
            placeholder="Leita að bjór"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          {filtered.length === 0 && (
            <p data-testid="no-match">Enginn bjór fannst.</p>
          )}
        </>
      ) : (
        <p data-testid="no-beers">Engir bjórar eru skráðir á tímabilið enn.</p>
      )}
    </HressPage>
  );
};

export default SeasonBeerList;
