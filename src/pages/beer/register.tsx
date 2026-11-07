import { useEffect, useRef, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import LoginRedirect from "./LoginRedirect";
import BeerMatches from "./components/BeerMatches";
import { HressPage, useHressAuth } from "./hress";
import { useDebouncedValue } from "./hooks/useDebouncedValue";
import {
  isSeasonClosed,
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
  const [nameError, setNameError] = useState<string | null>(null);
  const [duplicate, setDuplicate] = useState<Beer | null>(null);
  const nameInput = useRef<HTMLInputElement>(null);
  const duplicatePanel = useRef<HTMLDivElement>(null);
  const matches = debouncedName.trim().length >= 2 ? (search.data ?? []) : [];

  // Bring the existing beer into view; the form is long on a phone.
  useEffect(() => {
    if (duplicate) {
      duplicatePanel.current?.scrollIntoView?.({
        behavior: "smooth",
        block: "nearest",
      });
    }
  }, [duplicate]);

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
          ref={nameInput}
          type="text"
          value={name}
          maxLength={100}
          autoComplete="off"
          onChange={(e) => {
            setName(e.target.value);
            setNameError(null);
            setDuplicate(null);
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
        <div ref={duplicatePanel}>
          <p>Þessi bjór er þegar til:</p>
          <BeerMatches
            matches={[{ beer: duplicate }]}
            onAdd={addBeer}
            adding={add.isPending}
          />
        </div>
      )}
      {add.isError && (
        <p role="alert">
          {isSeasonClosed(add.error)
            ? "Tímabilið er búið og ekki hægt að skrá bjór."
            : "Ekki tókst að bæta bjórnum við. Reyndu aftur."}
        </p>
      )}
      <NewBeerForm
        seasonID={season.id}
        name={name}
        onNameError={setNameError}
        onSaved={(beer) => navigate(`/beer/${encodeURIComponent(beer.id)}`)}
        onDuplicate={setDuplicate}
        onSubmitStart={() => setDuplicate(null)}
        focusName={() => nameInput.current?.focus()}
      />
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
