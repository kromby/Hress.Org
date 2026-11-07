# Add a beer — design

Covers kromby/BeerRater#8 (BR-10 Add a beer), #9 (BR-11 Search before adding) and #10
(BR-12 Add a beer from earlier seasons to this season).

## Why

During a season, members bring beers that aren't on the list yet. Today only the owner can
add beers, by hand in Storage Explorer. The season list already has a "Skrá nýjan bjór"
button, but it links to `/beer/register`, which doesn't exist (the `/beer/:beerID` route
catches it and says the beer isn't in the season).

The typical moment: a member at the table, on their phone, **holding the can** (sometimes
with a Vínbúðin product page open). They type what's on the label. Christmas beers come back
every year, so the beer they're holding is often already in the catalog from an earlier
season.

## Goals

- A member can add a beer to the current season from their phone in under a minute.
- If the beer is already in the catalog (any season), they find it while typing the name
  and add it to this season unchanged, instead of creating a duplicate.
- They can take a photo of the can, which becomes the beer's cover photo.
- After adding, they land on the beer's page, ready to rate and chat.

## Non-goals

- Filling the form from a Vínbúðin link (BR-16) — later.
- Editing or fixing a beer after it's added (BR-15) — later. The owner fixes rows, and swaps
  bad cover photos, by hand.
- Price fields.
- Adding beers to a closed season, or to any season other than the current one.

## Anti-goals

- Duplicates: the same beer twice in the catalog because the member didn't see it was
  already there.
- A beer saved with a broken or missing photo when the member did pick one.
- Losing what the member typed because of a failed upload or save.

## Decided by the owner

- **Photo:** the member can take a photo of the can when adding a beer. It's uploaded to the
  hress.org **album 46153**, and its image ID becomes the beer's cover (`ImageID`).
- **Existing beers:** matches from the whole catalog show while typing the name; choosing one
  adds it to the current season **unchanged** and opens its page.
- **Fields:** name, brewery (producer), country and alcohol % are required. Vínbúðin link,
  style ("Tegund") and description ("Lýsing") are optional. Alcohol % must be above 0 and at
  most 20. (The issue's "no style field in the MVP" criterion is out of date: beers now have
  Style and Description.)
- **Layout:** the approved mockup, in the hress.org post style:
  - Page title "Skrá nýjan bjór", with the season name as the description.
  - **Name field first**, with the matches listed under it as you type. Each match shows its
    cover thumbnail, name, "producer · alcohol %" and which season it was in, plus a
    "Bæta við" button. A beer that's already in the current season says so ("er á
    tímabilinu") and has no button.
  - The rest of the form: photo ("Taka mynd af dósinni") at the top, then name, brewery,
    country and alcohol % side by side, the Vínbúðin link, style, description, and
    "Skrá bjór".
  - Phone-first, no horizontal scroll at 375px.

## Constraints

- **Code conventions:** Hress.Org conventions, as in CLAUDE.md and ARCHITECTURE_RULES.md:
  - Domain logic in `Ez.Hress.BeerRater` interactors, storage behind `I…DataAccess`.
  - Functions in `Ez.Hress.FunctionsApi/Beer/`.
  - JWT via `X-Custom-Authorization`.
  - xUnit + Moq tests.
- **Frontend boundary:** beer pages live in `src/pages/beer/`. `src/pages/beer/hress.tsx` is
  the only file there that may use the rest of hress.org (`Post`, auth, image URLs). Image
  upload and album hooks are reached through it.
- **Storage:** beers live in the Azure Table `Beers` (PartitionKey `"beer"`, RowKey = beer ID,
  URL-encoded). Season membership lives in `BeerSeasonBeers`, through the existing
  `IBeerSeasonDataAccess.AddBeerToSeasonAsync`.
- **Photo upload:** *(My call.)* Use the `Album` image container, as album uploads do, not the
  unused `ImageContainer.Beer`. The photo belongs to album 46153 anyway, and a missing `beer`
  blob container in production would make every upload fail. The existing two calls, both
  open to any logged-in member:
  1. `POST /api/images` saves the image and returns the `ImageEntity` with its ID.
  2. `POST /api/albums/46153/images` with `{ imageId }` adds it to the album.

  `src/hooks/useImages` (`uploadImage`) and `src/hooks/useAlbums` (`addImage`) wrap them;
  `src/pages/albums/albumImageUpload.tsx` shows the usage, including the 10 MB limit and the
  `image/*` check.
- **API JSON:** responses use `NullValueHandling.Ignore` and `DefaultValueHandling.Ignore`.
- **Season state:** writes only while the season is open, like rating and chat
  (`SeasonInteractor.GetOpenSeasonAsync` throws `SeasonClosedException` → 409 elsewhere).

## How it works

### Page: `/beer/register`

- New route inside the existing `beer` routes in `src/pages/App.js`, **before** `:beerID`,
  wrapped in `RequireLogin`. The season list's "Skrá nýjan bjór" button already links here.
- Loads the current season. If it's closed, the page says beers can't be added any more
  (the button is already hidden on a closed season, so this is for direct links).
- **Name field with matches:**
  - As the member types (from 2 characters, debounced), it lists catalog beers whose name
    matches, ignoring case and accents. They come from the new catalog search endpoint below.
  - Each match shows its thumbnail (`BeerCover`), name, "producer · alcohol %" and the last
    season it was in.
  - A beer already in the current season says "er á tímabilinu" and links to its page
    instead of offering "Bæta við".
  - "Bæta við" calls the add-to-season endpoint and navigates to `/beer/{id}`.
  - Under the matches, a hint says to keep filling in the form if the beer isn't there.
- **Photo:**
  - `<input type="file" accept="image/*" capture="environment">` shows as a "Taka mynd af
    dósinni" button. On phones it opens the camera.
  - After picking, show a preview with a way to remove or retake it.
  - Same 10 MB / `image/*` validation as the album upload.
- **Saving ("Skrá bjór"):**
  1. Validate the fields on the client, with the same rules as the API.
  2. If there's a photo, upload it (`POST /api/images`), then add it to album 46153.
  3. Create the beer with the returned image ID (endpoint below). The API adds it to the
     current season.
  4. Navigate to `/beer/{newID}`, and invalidate the season's beer list query so the list
     shows it.

  If any step fails, show an error, keep everything typed (and the photo), and allow
  retrying. If the photo uploaded but the beer didn't save, retrying reuses the uploaded
  image ID instead of uploading again.

  *(My call.)* Adding the photo to album 46153 is best-effort: if that call fails, log it and
  still create the beer with the image as its cover. The cover is what the member sees; album
  membership is for the owner, who can add the image to the album by hand. The page
  remembers which steps succeeded, so a retry never uploads the image or adds it to the album
  twice.
- Disable the button while saving, to prevent double submits.

### API

All under the existing BeerRater module and `beer/…` routes, JWT required.

1. **Search the catalog:** `GET /api/beer/beers?query=…`
   - Returns catalog beers whose name contains the query, ignoring case and diacritics
     (Icelandic "á" matches "a", "ð" matches "d"). Sorted by name in Icelandic order, at most
     10.
   - Each result includes the beer, the seasons it's in (or at least the latest one, with its
     name), and whether it's in the current season, so the page can label it.
2. **Add an existing beer to the current season:** `POST /api/beer/seasons/{seasonID}/beers`
   with `{ "beerID": "…" }`.
   - 404 for an unknown beer or season; 409 for a closed season.
   - Adding a beer that's already in the season succeeds without duplicating it.
   - Returns the beer's summary.
3. **Create a beer:** `POST /api/beer/seasons/{seasonID}/beers` with a `beer` object instead
   of `beerID`. A separate route is fine if the builder prefers.
   - Fields: name, producer, country, alcoholPercentage, vinbudinUrl?, style?, description?,
     imageID?.
   - Validates: required fields not blank after trimming; alcohol % > 0 and ≤ 20; vinbudinUrl,
     if given, an absolute https URL; sensible maximum lengths. Errors → 400.
   - Sets `RegisteredBy` (from the token) and `RegisteredAt` (UTC now). Saves to `Beers`,
     then adds it to the season. 409 if the season is closed.
   - *(My call.)* Duplicate guard: if a catalog beer has the same name and producer after
     normalizing (trimmed, case- and diacritic-insensitive, whitespace collapsed), return 409
     with that beer. The page then shows it as a match with "Bæta við" instead of saving a
     copy.
   - Returns the new beer, including its ID.
   - `IBeerDataAccess` gains a method to add a beer, failing if the ID exists; the table
     mapping already covers every field.

### Issues

When done, close BeerRater #8, #9 and #10. Update #8's criteria: style and description are
optional fields, and the photo goes to album 46153.

## Left to the builder

- **Beer ID scheme.** *(My call; the owner didn't say whether IDs like `31696` are Vínbúðin
  product numbers.)* New beers get a short, URL-safe random ID. Reusing the Vínbúðin number
  can be added later without touching existing rows.
- Whether search filters in the API over the full `Beers` partition (fine at this size, a few
  hundred rows) or something smarter.
- The exact debounce, minimum query length and maximum lengths.
- Whether "add existing" and "create" share one endpoint or use two.
- Component structure within `src/pages/beer/`, following `SeasonBeerList`, `BeerPost` and
  `BeerCover`.
- Tests:
  - API: interactor tests for validation, closed season, duplicate add, and search matching
    (case and diacritics).
  - Frontend: the matches list, the add-existing flow, the create flow with and without a
    photo, failure and retry without losing input, and the closed season.
