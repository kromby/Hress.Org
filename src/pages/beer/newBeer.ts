import { NewBeerInput } from "./types";

export interface NewBeerValues {
  name: string;
  producer: string;
  country: string;
  // As typed, so a comma decimal ("5,4") is allowed.
  alcohol: string;
  vinbudinUrl: string;
  style: string;
  description: string;
}

export type NewBeerErrors = Partial<Record<keyof NewBeerValues, string>>;

export const MAX_LENGTHS = {
  name: 100,
  producer: 100,
  country: 100,
  style: 100,
  vinbudinUrl: 500,
  description: 2000,
} as const;

const isHttpsUrl = (value: string): boolean => {
  try {
    return new URL(value).protocol === "https:";
  } catch {
    return false;
  }
};

export const validateNewBeer = (
  values: NewBeerValues
): { input?: NewBeerInput; errors: NewBeerErrors } => {
  const errors: NewBeerErrors = {};
  const name = values.name.trim();
  const producer = values.producer.trim();
  const country = values.country.trim();
  const vinbudinUrl = values.vinbudinUrl.trim();
  const style = values.style.trim();
  const description = values.description.trim();

  if (!name) errors.name = "Nafn er nauðsynlegt";
  else if (name.length > MAX_LENGTHS.name)
    errors.name = `Nafn má ekki vera lengra en ${MAX_LENGTHS.name} stafir`;

  if (!producer) errors.producer = "Brugghús er nauðsynlegt";
  else if (producer.length > MAX_LENGTHS.producer)
    errors.producer = `Brugghús má ekki vera lengra en ${MAX_LENGTHS.producer} stafir`;

  if (!country) errors.country = "Land er nauðsynlegt";
  else if (country.length > MAX_LENGTHS.country)
    errors.country = `Land má ekki vera lengra en ${MAX_LENGTHS.country} stafir`;

  const alcoholText = values.alcohol.trim().replace(",", ".");
  const alcohol = Number(alcoholText);
  if (!alcoholText) errors.alcohol = "Styrkleiki er nauðsynlegur";
  else if (!Number.isFinite(alcohol) || alcohol <= 0 || alcohol > 20)
    errors.alcohol = "Styrkleiki þarf að vera yfir 0 og í mesta lagi 20";

  if (vinbudinUrl) {
    if (vinbudinUrl.length > MAX_LENGTHS.vinbudinUrl)
      errors.vinbudinUrl = "Slóðin er of löng";
    else if (!isHttpsUrl(vinbudinUrl))
      errors.vinbudinUrl = "Slóðin þarf að byrja á https://";
  }

  if (style.length > MAX_LENGTHS.style)
    errors.style = `Tegund má ekki vera lengri en ${MAX_LENGTHS.style} stafir`;
  if (description.length > MAX_LENGTHS.description)
    errors.description = `Lýsing má ekki vera lengri en ${MAX_LENGTHS.description} stafir`;

  if (Object.keys(errors).length > 0) return { errors };

  return {
    errors,
    input: {
      name,
      producer,
      country,
      alcoholPercentage: alcohol,
      vinbudinUrl: vinbudinUrl || undefined,
      style: style || undefined,
      description: description || undefined,
    },
  };
};
