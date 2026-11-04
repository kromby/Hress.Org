import { useEffect } from "react";
import { Link, useParams } from "react-router-dom";
import LoginRedirect from "./LoginRedirect";
import BeerRatingStars from "./components/BeerRatingStars";
import { HressPage, useHressAuth } from "./hress";
import {
  isUnauthorized,
  useCurrentSeason,
  useSeasonBeers,
} from "./hooks/useBeerApi";

const formatAlcohol = (percentage = 0) =>
  `${percentage.toLocaleString("is-IS", { maximumFractionDigits: 1 })}%`;

const BackLink = () => <Link to="/beer">← Allir bjórar</Link>;

// A beer in the current season. It's read from the season's beer list,
// which holds the details and the user's own rating.
const BeerPage = () => {
  const { beerID } = useParams();
  const { signOut } = useHressAuth();
  const season = useCurrentSeason();
  const beers = useSeasonBeers(season.data?.id);
  const error = season.error ?? beers.error;
  const unauthorized = isUnauthorized(error);

  // The API rejected the stored token. Clear it, otherwise the login page
  // sees a token and sends the user straight back here.
  useEffect(() => {
    if (unauthorized) signOut();
  }, [unauthorized]);

  if (unauthorized) {
    return <LoginRedirect />;
  }

  if (season.isLoading || beers.isLoading) {
    return (
      <HressPage title="Bjór">
        <p data-testid="beer-loading">Sæki bjór…</p>
      </HressPage>
    );
  }

  if (error) {
    return (
      <HressPage title="Bjór">
        <p data-testid="beer-error">Ekki tókst að sækja bjórinn.</p>
        <BackLink />
      </HressPage>
    );
  }

  const summary = beers.data?.find((s) => s.beer.id === beerID);
  if (!season.data || !summary) {
    return (
      <HressPage title="Bjór">
        <p data-testid="beer-not-found">Þessi bjór er ekki á tímabilinu.</p>
        <BackLink />
      </HressPage>
    );
  }

  const { beer, myRating } = summary;

  return (
    <HressPage title={beer.name} description={season.data.name}>
      <p data-testid="beer-details">
        {beer.producer} · {beer.country} ·{" "}
        {formatAlcohol(beer.alcoholPercentage)}
        {beer.vinbudinUrl && (
          <>
            <br />
            <a href={beer.vinbudinUrl} target="_blank" rel="noreferrer">
              Skoða í Vínbúðinni
            </a>
          </>
        )}
      </p>
      <BeerRatingStars
        season={season.data}
        beerID={beer.id}
        myRating={myRating}
      />
      <BackLink />
    </HressPage>
  );
};

export default BeerPage;
