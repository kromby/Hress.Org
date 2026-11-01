import { useEffect } from "react";
import LoginLink from "./LoginLink";
import { useHressAuth } from "./hress";
import { isUnauthorized, useCurrentSeason } from "./hooks/useBeerApi";

const BeerHome = () => {
  const { signOut } = useHressAuth();
  const { data: season, isLoading, isError, error } = useCurrentSeason();
  const unauthorized = isError && isUnauthorized(error);

  // The API rejected the stored token. Clear it, otherwise the login page
  // sees a token and sends the user straight back here.
  useEffect(() => {
    if (unauthorized) signOut();
  }, [unauthorized]); // eslint-disable-line react-hooks/exhaustive-deps

  if (isLoading) {
    return <p data-testid="season-loading">Sæki tímabil…</p>;
  }

  if (unauthorized) {
    return (
      <section data-testid="login-required">
        <p>Innskráningin er útrunnin. Skráðu þig inn aftur.</p>
        <LoginLink />
      </section>
    );
  }

  if (isError) {
    return <p data-testid="season-error">Ekki tókst að sækja tímabilið.</p>;
  }

  if (!season) {
    return (
      <p data-testid="no-season">Ekkert bjórtímabil er í gangi núna.</p>
    );
  }

  return (
    <section>
      <h1>{season.name}</h1>
      {!season.isOpen && (
        <p data-testid="season-closed">Þetta tímabil er búið.</p>
      )}
    </section>
  );
};

export default BeerHome;
