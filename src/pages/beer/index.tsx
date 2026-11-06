import { useEffect } from "react";
import LoginRedirect from "./LoginRedirect";
import SeasonBeerList from "./components/SeasonBeerList";
import { HressPage, useHressAuth } from "./hress";
import { isUnauthorized, useCurrentSeason } from "./hooks/useBeerApi";

const BeerHome = () => {
  const { signOut } = useHressAuth();
  const { data: season, isLoading, isError, error } = useCurrentSeason();
  const unauthorized = isError && isUnauthorized(error);

  // The API rejected the stored token. Clear it, otherwise the login page
  // sees a token and sends the user straight back here.
  useEffect(() => {
    if (unauthorized) signOut();
  }, [unauthorized]);

  if (isLoading) {
    return (
      <HressPage title="Bjór">
        <p data-testid="season-loading">Sæki tímabil…</p>
      </HressPage>
    );
  }

  if (unauthorized) {
    return <LoginRedirect />;
  }

  if (isError) {
    return (
      <HressPage title="Bjór">
        <p data-testid="season-error">Ekki tókst að sækja tímabilið.</p>
      </HressPage>
    );
  }

  if (!season) {
    return (
      <HressPage title="Bjór">
        <p data-testid="no-season">Ekkert bjórtímabil er í gangi núna.</p>
      </HressPage>
    );
  }

  return <SeasonBeerList season={season} />;
};

export default BeerHome;
