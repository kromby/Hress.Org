import LoginLink from "./LoginLink";
import { isUnauthorized, useCurrentSeason } from "./hooks/useBeerApi";

const BeerHome = () => {
  const { data: season, isLoading, isError, error } = useCurrentSeason();

  if (isLoading) {
    return <p data-testid="season-loading">Sæki tímabil…</p>;
  }

  if (isError && isUnauthorized(error)) {
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
