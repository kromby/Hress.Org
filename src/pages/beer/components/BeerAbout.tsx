import { Beer } from "../types";

const formatAlcohol = (percentage = 0) =>
  `${percentage.toLocaleString("is-IS", { maximumFractionDigits: 1 })}%`;

interface BeerAboutProps {
  beer: Beer;
}

// About the beer, laid out like the Hardhead night and movie info: a heading,
// the description, and the facts in a grid of labelled cells.
const BeerAbout = ({ beer }: BeerAboutProps) => {
  const facts = [
    { label: "Tegund", value: beer.style },
    { label: "Brugghús", value: beer.producer },
    { label: "Land", value: beer.country },
    { label: "Styrkleiki", value: formatAlcohol(beer.alcoholPercentage) },
  ].filter((fact) => fact.value);

  return (
    <section>
      <h3>Bjórinn</h3>
      {beer.description && (
        // Kept as text, with its line breaks.
        <p data-testid="beer-description" style={{ whiteSpace: "pre-wrap" }}>
          {beer.description}
        </p>
      )}
      <div
        data-testid="beer-details"
        className="row"
        style={{ marginBottom: "2em" }}
      >
        {facts.map(({ label, value }) => (
          <div key={label} className="col-3 col-6-small">
            <strong>{label}</strong>
            <br />
            {value}
          </div>
        ))}
      </div>
      {beer.vinbudinUrl && (
        <p>
          <a href={beer.vinbudinUrl} target="_blank" rel="noreferrer">
            Skoða í Vínbúðinni
          </a>
        </p>
      )}
    </section>
  );
};

export default BeerAbout;
