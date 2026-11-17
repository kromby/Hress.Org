import { FormEvent, useRef, useState } from "react";
import { useHressPhotos } from "../hress";
import {
  DuplicateBeerError,
  isSeasonClosed,
  useCreateBeer,
} from "../hooks/useBeerApi";
import { MAX_LENGTHS, NewBeerErrors, validateNewBeer } from "../newBeer";
import { Beer } from "../types";
import BeerPhotoInput from "./BeerPhotoInput";

interface NewBeerFormProps {
  seasonID: string;
  // The page's "Nafn" field is the new beer's name.
  name: string;
  // Reports a validation error for the name, shown under that field.
  onNameError: (error: string | null) => void;
  onSaved: (beer: Beer) => void;
  onDuplicate: (beer: Beer) => void;
  // Called when a submit starts, so the page can drop stale feedback.
  onSubmitStart: () => void;
  // Focuses the page's "Nafn" field.
  focusName: () => void;
}

const FIELD_IDS: [keyof NewBeerErrors, string][] = [
  ["producer", "beer-producer"],
  ["country", "beer-country"],
  ["alcohol", "beer-alcohol"],
  ["vinbudinUrl", "beer-vinbudin"],
  ["style", "beer-style"],
  ["description", "beer-description"],
];

const FieldError = ({ message }: { message?: string }) =>
  message ? <small role="alert">{message}</small> : null;

// Registers a beer that isn't in the catalog yet, with a photo of the can.
const NewBeerForm = ({
  seasonID,
  name,
  onNameError,
  onSaved,
  onDuplicate,
  onSubmitStart,
  focusName,
}: NewBeerFormProps) => {
  const formRef = useRef<HTMLFormElement>(null);
  const create = useCreateBeer(seasonID);
  const { uploadPhoto, addToAlbum } = useHressPhotos();
  const [producer, setProducer] = useState("");
  const [country, setCountry] = useState("");
  const [alcohol, setAlcohol] = useState("");
  const [vinbudinUrl, setVinbudinUrl] = useState("");
  const [style, setStyle] = useState("");
  const [description, setDescription] = useState("");
  const [photo, setPhoto] = useState<File | null>(null);
  // Kept so a retry skips the steps that already worked.
  const [uploadedImageID, setUploadedImageID] = useState<number | null>(null);
  const [addedToAlbum, setAddedToAlbum] = useState(false);
  const [errors, setErrors] = useState<NewBeerErrors>({});
  const [saving, setSaving] = useState(false);
  const [failure, setFailure] = useState<"closed" | "failed" | null>(null);

  const changePhoto = (file: File | null) => {
    setPhoto(file);
    setUploadedImageID(null);
    setAddedToAlbum(false);
  };

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    if (saving) return;
    setFailure(null);
    onSubmitStart();

    const result = validateNewBeer({
      name,
      producer,
      country,
      alcohol,
      vinbudinUrl,
      style,
      description,
    });
    setErrors(result.errors);
    onNameError(result.errors.name ?? null);
    if (!result.input) {
      if (result.errors.name) focusName();
      else {
        const first = FIELD_IDS.find(([key]) => result.errors[key]);
        if (first)
          formRef.current?.querySelector<HTMLElement>(`#${first[1]}`)?.focus();
      }
      return;
    }

    setSaving(true);
    try {
      let imageID = uploadedImageID;
      if (photo && imageID === null) {
        imageID = await uploadPhoto(photo, result.input.name);
        setUploadedImageID(imageID);
      }
      if (imageID !== null && !addedToAlbum) {
        try {
          await addToAlbum(imageID);
          setAddedToAlbum(true);
        } catch (error) {
          console.error(
            "[NewBeerForm.submit] Could not add photo to album",
            error
          );
        }
      }
      const beer = await create.mutateAsync({
        ...result.input,
        imageID: imageID ?? undefined,
      });
      onSaved(beer);
    } catch (error) {
      if (error instanceof DuplicateBeerError) onDuplicate(error.existing);
      else setFailure(isSeasonClosed(error) ? "closed" : "failed");
    } finally {
      setSaving(false);
    }
  };

  return (
    <form ref={formRef} onSubmit={submit} noValidate>
      {/* One grid for every field, so the gaps between them match. */}
      <div className="row gtr-uniform">
        <div className="col-12">
          <BeerPhotoInput file={photo} onChange={changePhoto} />
        </div>
        <div className="col-12">
          <label htmlFor="beer-producer">Brugghús</label>
          <input
            id="beer-producer"
            type="text"
            value={producer}
            maxLength={MAX_LENGTHS.producer}
            onChange={(e) => setProducer(e.target.value)}
          />
          <FieldError message={errors.producer} />
        </div>
        <div className="col-6 col-12-small">
          <label htmlFor="beer-country">Land</label>
          <input
            id="beer-country"
            type="text"
            value={country}
            maxLength={MAX_LENGTHS.country}
            onChange={(e) => setCountry(e.target.value)}
          />
          <FieldError message={errors.country} />
        </div>
        <div className="col-6 col-12-small">
          <label htmlFor="beer-alcohol">Styrkleiki %</label>
          <input
            id="beer-alcohol"
            type="text"
            inputMode="decimal"
            value={alcohol}
            onChange={(e) => setAlcohol(e.target.value)}
          />
          <FieldError message={errors.alcohol} />
        </div>
        <div className="col-12">
          <label htmlFor="beer-vinbudin">Vínbúðin (valfrjálst)</label>
          <input
            id="beer-vinbudin"
            // Text, not url: the theme only styles text inputs. inputMode still
            // brings up the URL keyboard on phones.
            type="text"
            inputMode="url"
            value={vinbudinUrl}
            maxLength={MAX_LENGTHS.vinbudinUrl}
            onChange={(e) => setVinbudinUrl(e.target.value)}
          />
          <FieldError message={errors.vinbudinUrl} />
        </div>
        <div className="col-12">
          <label htmlFor="beer-style">Tegund (valfrjálst)</label>
          <input
            id="beer-style"
            type="text"
            value={style}
            maxLength={MAX_LENGTHS.style}
            onChange={(e) => setStyle(e.target.value)}
          />
          <FieldError message={errors.style} />
        </div>
        <div className="col-12">
          <label htmlFor="beer-description">Lýsing (valfrjálst)</label>
          <textarea
            id="beer-description"
            rows={4}
            value={description}
            maxLength={MAX_LENGTHS.description}
            onChange={(e) => setDescription(e.target.value)}
          />
          <FieldError message={errors.description} />
        </div>
        <div className="col-12">
          {failure === "closed" && (
            <p role="alert">Tímabilið er búið og ekki hægt að skrá bjór.</p>
          )}
          {failure === "failed" && (
            <p role="alert">Ekki tókst að skrá bjórinn. Reyndu aftur.</p>
          )}
          <button type="submit" className="button primary" disabled={saving}>
            Skrá bjór
          </button>
        </div>
      </div>
    </form>
  );
};

export default NewBeerForm;
