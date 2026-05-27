# Rating Save Migration — Legacy API → New API

**Issue:** [#672](https://github.com/kromby/Hress.Org/issues/672)
**Date:** 2026-05-27
**Status:** Approved (design)

## Background

The site is migrating off the legacy Azure Web API (`REACT_APP_LEGACY_API_PATH` → `ezhressapi.azurewebsites.net`) onto the new Azure Functions API in `Hress.Org/api/` (`Ez.Hress.FunctionsApi`).

Rating reads have already moved: `useRatings.ts:14` calls `${config.get("apiPath")}/api/hardhead/${id}/ratings` with the `X-Custom-Authorization` header. Rating saves are still on the legacy API: `rating.tsx:45–57` posts to `${config.get("path")}/api/hardhead/${id}/ratings?code=...` with the `Authorization` header.

The issue framed this as a one-line URL swap. It is not: the new API's POST handler at `api/Ez.Hress.FunctionsApi/Hardhead/HardheadRatingFunctions.cs:48` throws `NotImplementedException`. The save path must be built end-to-end on the new API before the frontend can be migrated.

## Goal

End-to-end rating save (night and movie) running through the new API, with the legacy POST no longer referenced from the frontend.

## Non-goals

- Refactoring the rating GET path (already migrated).
- Splitting ratings into a separate `RatingInteractor` module (the new codebase deliberately colocates rating concerns inside `HardheadInteractor`; a future refactor would need to move both get and save together).
- Integration tests against the database.
- Function-level tests for the new POST handler.

## Design

### 1. Backend data access

`api/Ez.Hress.Hardhead/UseCases/IHardheadDataAccess.cs` — add two methods:

```csharp
Task<int> InsertRatingAsync(int eventId, int userId, string typeCode, int rating);
Task<int> UpdateRatingAsync(int eventId, int userId, string typeCode, int rating);
```

`api/Ez.Hress.Hardhead/DataAccess/HardheadSqlAccess.cs` — implement both, ported from the legacy `Ez.Hress/Hardhead/Ez.Hress.HardheadDataAccess/HardheadSqlAccess.cs`:

- **InsertRatingAsync** — `INSERT INTO rep_Count (EventId, TypeId, Count, Inserted, InsertedBy) VALUES (@hardheadID, (SELECT t.ID FROM gen_Type t WHERE t.Shortcode = @ratingCode), @rating, GETDATE(), @userID)`. Returns rows-affected.
- **UpdateRatingAsync** — `UPDATE rep_Count SET Count = @rating, Updated = GETDATE(), UpdatedBy = @userID WHERE EventId = @id AND InsertedBy = @userId AND TypeId = (SELECT Id FROM gen_Type WHERE Shortcode = @typeCode)`. Returns rows-affected.

Existence is determined by reusing the existing `GetMyRatingAsync(id, userId)`, which already returns `{ typeCode → ratingValue }`. No new `GetRatingIdAsync` is needed; the legacy `WHERE Id = @ratingID` shape is replaced by the composite-key update above.

Naming follows the `*Async` convention already used in this file (`GetMyRatingAsync`, `GetAverageRatingAsync`).

### 2. Backend interactor

`api/Ez.Hress.Hardhead/UseCases/HardheadInteractor.cs` — add:

```csharp
public async Task<bool> SaveRatingAsync(int id, int userId, string typeCode, int rating)
```

Logic, ported from legacy with corrections:

1. **Validate.** `rating` must be in `1..5`; `typeCode` must be one of `"REP_C_RTNG"` or `"REP_C_MRTNG"`. On violation, throw `ArgumentException` (mapped to 400 by the existing function-level handler).
2. **Existence check.** `var existing = await _hardheadDataAccess.GetMyRatingAsync(id, userId);`
3. **Update branch.** If `existing.ContainsKey(typeCode)` → call `UpdateRatingAsync(id, userId, typeCode, rating)`. Return `affected == 1`.
4. **Insert branch.** Otherwise → `var guests = await _hardheadDataAccess.GetGuests(id);` and if `!guests.Any(g => g.ID == userId)` return `false` (user did not attend → cannot rate). Else call `InsertRatingAsync(id, userId, typeCode, rating)`. Return `affected == 1`.

Bug-for-bug note: the legacy `SaveRating` called `hardheadDataAccess.GetGuests(ID)` without `await` and used the returned `Task<IList<>>` as if it were `IList<>`. The new method awaits properly.

Return contract: `true` on successful insert/update; `false` only on the "user did not attend" reject. Validation failures throw.

### 3. Backend function

`api/Ez.Hress.FunctionsApi/Hardhead/HardheadRatingFunctions.cs:48` — replace the `throw new NotImplementedException()` with the POST handler:

```csharp
if (HttpMethods.IsPost(req.Method))
{
    if (!isJWTValid)
        return new UnauthorizedResult();

    if (id <= 0)
        return new BadRequestObjectResult("Invalid ID");

    var body = await JsonSerializer.DeserializeAsync<RatingSaveRequest>(
        req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    if (body is null || string.IsNullOrWhiteSpace(body.Type))
        return new BadRequestObjectResult("Missing type or rating");

    var ok = await _hardheadInteractor
        .SaveRatingAsync(id, userID, body.Type, body.Rating)
        .ConfigureAwait(false);

    return ok
        ? new CreatedResult(string.Empty, null)
        : new BadRequestObjectResult("Save failed");
}
```

Add a small DTO record alongside the function (private/internal, in the same file or `Entities/`):

```csharp
internal sealed record RatingSaveRequest(string Type, int Rating);
```

Status code mapping (mirrors the legacy `Ez.Hress/Ez.Hress.Functions/HardheadFunction.cs` POST handler):

| Condition | Status |
|---|---|
| Missing / invalid JWT | 401 |
| `id <= 0`, missing/empty body, missing `type` | 400 |
| `ArgumentException` from interactor (invalid rating / typeCode) | 400 (already caught by existing handler) |
| `SaveRatingAsync` returns `false` (user did not attend) | 400 |
| Save succeeded | 201 |
| Unhandled exception | 500 (already caught) |

### 4. Frontend

#### `src/pages/hardhead/hooks/useRatings.ts`

Co-locate save with read. Add:

```ts
const saveRating = async (rate: number, type: string) => {
  if (!authTokens) return;
  const url = `${config.get("apiPath")}/api/hardhead/${id}/ratings`;
  await axios.post(
    url,
    { type, rating: rate },
    { headers: { "X-Custom-Authorization": `token ${authTokens.token}` } }
  );
  await getRatingData();
};

return { ratings, refreshRatings: getRatingData, saveRating };
```

The hook lets errors propagate; the caller owns the UX response.

#### `src/pages/hardhead/components/rating.tsx`

- Replace the destructure `const { ratings, refreshRatings } = useRatings(id);` with `const { ratings, saveRating } = useRatings(id);`
- Delete the inline `saveRating` (lines 42–65) and replace with a thin handler that owns the try/catch + alert:

```ts
const handleSaveRating = async (rate: number, type: string) => {
  try {
    await saveRating(rate, type);
  } catch (e) {
    console.error(e);
    alert("Ekki tókst að vista einkunn, reyndu aftur síðar.");
  }
};
```

- Wire `handleSaveRating` into the `changeRating` prop (current line 102).
- Remove now-unused imports: `axios`, `config` (`react-global-configuration`).

After the edits, this command should return nothing:

```sh
grep -nE 'config\.get\("path"\)|config\.get\("code"\)' src/pages/hardhead/components/rating.tsx
```

### 5. Tests

Create `api/Ez.Hress.UnitTest/Hardhead/HardheadInteractorTests.cs` (no existing file for this interactor; follow the style of sibling tests like `MovieInteractorTests.cs`). The project uses **xUnit 2.9** and **Moq 4.20** (per `api/Ez.Hress.UnitTest/Ez.Hress.UnitTest.csproj`). Add tests for `SaveRatingAsync`, mocking `IHardheadDataAccess`. Four cases:

1. **Existing rating → update path.** `GetMyRatingAsync` returns a dict containing `typeCode`. Expect `UpdateRatingAsync` called once with the right args; `InsertRatingAsync` and `GetGuests` not called; result is `true` when affected == 1.
2. **No existing rating + attended → insert path.** `GetMyRatingAsync` returns empty; `GetGuests` returns a list containing `userId`. Expect `InsertRatingAsync` called; `UpdateRatingAsync` not called; result `true`.
3. **No existing rating + did not attend → reject.** `GetMyRatingAsync` empty; `GetGuests` does not include `userId`. Result is `false`; neither insert nor update is called.
4. **Invalid input → `ArgumentException`.** Cover at least: `rating == 0`, `rating == 6`, unknown `typeCode`. Use `[Theory]` with `[InlineData]` for the rating bounds.

### 6. Cleanup

Delete the placeholder files that are fully commented out and no longer reflect the chosen architecture:

- `api/Ez.Hress.Hardhead/UseCases/IRatingDataAccess.cs`
- `api/Ez.Hress.Hardhead/UseCases/RatingInteractor.cs`

These were never wired into DI and are confusing dead code now that ratings live on `HardheadInteractor` / `IHardheadDataAccess`.

## Acceptance

- `saveRating` POSTs to `${apiPath}/api/hardhead/${id}/ratings` with `X-Custom-Authorization: token ...`.
- Rating save works end-to-end for both night rating (`REP_C_RTNG`) and movie rating (`REP_C_MRTNG`), for both first-time save (insert) and re-rating (update).
- A non-attendee receives 400 from the API.
- A missing or invalid JWT receives 401.
- No remaining references to `config.get("path")` or `config.get("code")` in `rating.tsx`.
- Unit tests for `SaveRatingAsync` pass (four cases above).
- `IRatingDataAccess.cs` and `RatingInteractor.cs` are deleted.

## Risk and rollback

- **Schema:** the port targets the existing `rep_Count` / `gen_Type` tables; no migration required. The legacy save has been writing here for years.
- **Rollback:** revert the frontend commit. The legacy endpoint will continue to accept saves until decommissioned independently.
