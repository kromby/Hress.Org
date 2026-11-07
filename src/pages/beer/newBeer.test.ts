import { NewBeerValues, validateNewBeer } from "./newBeer";

const valid: NewBeerValues = {
  name: "Jóla Gull",
  producer: "Ölgerðin",
  country: "Ísland",
  alcohol: "5,4",
  vinbudinUrl: "",
  style: "",
  description: "",
};

describe("validateNewBeer", () => {
  it("accepts a comma decimal", () => {
    const { input, errors } = validateNewBeer(valid);
    expect(errors).toEqual({});
    expect(input?.alcoholPercentage).toBe(5.4);
  });

  it("requires the four fields", () => {
    const { input, errors } = validateNewBeer({
      ...valid,
      name: "",
      producer: " ",
      country: "",
      alcohol: "",
    });
    expect(input).toBeUndefined();
    expect(Object.keys(errors).sort()).toEqual([
      "alcohol",
      "country",
      "name",
      "producer",
    ]);
  });

  it("checks the alcohol range", () => {
    expect(
      validateNewBeer({ ...valid, alcohol: "0" }).errors.alcohol
    ).toBeTruthy();
    expect(
      validateNewBeer({ ...valid, alcohol: "20,1" }).errors.alcohol
    ).toBeTruthy();
    expect(
      validateNewBeer({ ...valid, alcohol: "20" }).errors.alcohol
    ).toBeUndefined();
  });

  it("checks the Vínbúðin link", () => {
    expect(
      validateNewBeer({ ...valid, vinbudinUrl: "vinbudin.is/x" }).errors
        .vinbudinUrl
    ).toBeTruthy();
    expect(
      validateNewBeer({ ...valid, vinbudinUrl: "https://www.vinbudin.is/x" })
        .errors.vinbudinUrl
    ).toBeUndefined();
  });

  it("drops blank optional fields", () => {
    const { input } = validateNewBeer({ ...valid, style: "  " });
    expect(input?.style).toBeUndefined();
    expect(input?.description).toBeUndefined();
    expect(input?.vinbudinUrl).toBeUndefined();
  });
});
