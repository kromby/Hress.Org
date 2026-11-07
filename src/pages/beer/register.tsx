import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import LoginRedirect from "./LoginRedirect";
import BeerMatches from "./components/BeerMatches";
import { HressPage, useHressAuth } from "./hress";
import { useDebouncedValue } from "./hooks/useDebouncedValue";
import {
  isUnauthorized,
  useAddBeerToSeason,
  useCatalogSearch,
  useCurrentSeason,
} from "./hooks/useBeerApi";
import NewBeerForm from "./components/NewBeerForm";
import { Beer, Season } from "./types";

const BackLink = () => (
  <p>
    <Link to="/beer">← Allir bjórar</Link>
  </p>
);

const RegisterForm = ({ season }: { season: Season }) => {
  const navigate = useNavigate();
  const [name, setName] = useState("");
  const debouncedName = useDebouncedValue(name, 300);
  const search = useCatalogSearch(debouncedName);
  const add = useAddBeerToSeason(season.id);
  const [nameError, setNameError] = useState<string>();
  const [duplicate, setDuplicate] = useState<Beer>();
  const matches = debouncedName.trim().length >= 2 ? (search.data ?? []) : [];

  const addBeer = (beerID: string) => {
    if (add.isPending) return;
    add.mutate(beerID, {
      onSuccess: (beer) => navigate(`/beer/${encodeURIComponent(beer.id)}`),
    });
  };

  return (
    <HressPage title="Skrá nýjan bjór" description={season.name}>
      <p>
        <label htmlFor="beer-name">Nafn</label>
        <input
          id="beer-name"
          type="text"
          value={name}
          maxLength={100}
          autoComplete="off"
          onChange={(e) => {
            setName(e.target.value);
            setNameError(undefined);
            setDuplicate(undefined);
            add.reset();
          }}
        />
        {nameError && <small role="alert">{nameError}</small>}
      </p>
      {matches.length > 0 && (
        <>
          <BeerMatches
            matches={matches}
            onAdd={addBeer}
            adding={add.isPending}
          />
          <p>Finnurðu hann ekki? Haltu áfram að skrá hann.</p>
        </>
      )}
      {duplicate && (
        <>
          <p>Þessi bjór er þegar til:</p>
          <BeerMatches
            matches={[{ beer: duplicate }]}
            onAdd={addBeer}
            adding={add.isPending}
          />
        </>
      )}
      <NewBeerForm
        seasonID={season.id}
        name={name}
        onNameError={setNameError}
        onSaved={(beer) => navigate(`/beer/${encodeURIComponent(beer.id)}`)}
        onDuplicate={setDuplicate}
      />
      {add.isError && (
        <p role="alert">Ekki tókst að bæta bjórnum við. Reyndu aftur.</p>
      )}
      <BackLink />
    </HressPage>
  );
};

// Registers a beer: type the name to find it in the catalog and add it to the
// open season.
const RegisterBeerPage = () => {
  const { signOut } = useHressAuth();
  const season = useCurrentSeason();
  const unauthorized = isUnauthorized(season.error);

  // The API rejected the stored token. Clear it, otherwise the login page
  // sees a token and sends the user straight back here.
  useEffect(() => {
    if (unauthorized) signOut();
  }, [unauthorized]);

  if (unauthorized) {
    return <LoginRedirect />;
  }

  if (season.isLoading) {
    return (
      <HressPage title="Skrá nýjan bjór">
        <p data-testid="register-loading">Sæki tímabil…</p>
      </HressPage>
    );
  }

  if (season.error) {
    return (
      <HressPage title="Skrá nýjan bjór">
        <p data-testid="register-error">Ekki tókst að sækja tímabilið.</p>
        <BackLink />
      </HressPage>
    );
  }

  if (!season.data || !season.data.isOpen) {
    return (
      <HressPage title="Skrá nýjan bjór" description={season.data?.name}>
        <p>Tímabilið er búið og ekki hægt að skrá bjór.</p>
        <BackLink />
      </HressPage>
    );
  }

  return <RegisterForm season={season.data} />;
};

export default RegisterBeerPage;
