import { useEffect } from "react";
import LoginLink from "./LoginLink";
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
    return (
      <HressPage title="Bjór">
        <section data-testid="login-required">
          <p>Innskráningin er útrunnin. Skráðu þig inn aftur.</p>
          <LoginLink />
        </section>
      </HressPage>
    );
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

  return (
    <HressPage title={season.name} description="Bjórar tímabilsins">
      {!season.isOpen && (
        <p data-testid="season-closed">Þetta tímabil er búið.</p>
      )}
      <SeasonBeerList season={season} />
    </HressPage>
  );
};

export default BeerHome;
