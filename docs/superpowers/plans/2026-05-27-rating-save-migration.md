# Rating Save Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** End-to-end rating save (night and movie) running through the new Azure Functions API, replacing the legacy POST that `rating.tsx` currently hits.

**Architecture:** Backend (xUnit-tested) adds `SaveRatingAsync` on `HardheadInteractor` plus `Insert/UpdateRatingAsync` on `IHardheadDataAccess` / `HardheadSqlAccess`, then fills in the POST branch in `HardheadRatingFunctions`. Frontend co-locates save with read in `useRatings.ts`; `rating.tsx` becomes UI-only.

**Tech Stack:** C# / .NET 9 / Azure Functions (isolated worker), xUnit 2.9 + Moq 4.20, React + TypeScript, axios.

**Spec:** `docs/superpowers/specs/2026-05-27-rating-save-migration-design.md`

---

## File Map

**Modify (backend):**
- `api/Ez.Hress.Hardhead/UseCases/IHardheadDataAccess.cs` — add `InsertRatingAsync`, `UpdateRatingAsync`
- `api/Ez.Hress.Hardhead/DataAccess/HardheadSqlAccess.cs` — implement both
- `api/Ez.Hress.Hardhead/UseCases/HardheadInteractor.cs` — add `SaveRatingAsync`
- `api/Ez.Hress.FunctionsApi/Hardhead/HardheadRatingFunctions.cs` — wire POST branch, add request DTO

**Create (tests):**
- `api/Ez.Hress.UnitTest/Hardhead/HardheadInteractorTests.cs` — four xUnit tests for `SaveRatingAsync`

**Delete (cleanup):**
- `api/Ez.Hress.Hardhead/UseCases/IRatingDataAccess.cs` (fully commented placeholder)
- `api/Ez.Hress.Hardhead/UseCases/RatingInteractor.cs` (fully commented placeholder)

**Modify (frontend):**
- `src/pages/hardhead/hooks/useRatings.ts` — return `saveRating`
- `src/pages/hardhead/components/rating.tsx` — consume hook's `saveRating`, drop inline POST

---

## Task 1: Add `InsertRatingAsync` to data access

**Files:**
- Modify: `api/Ez.Hress.Hardhead/UseCases/IHardheadDataAccess.cs`
- Modify: `api/Ez.Hress.Hardhead/DataAccess/HardheadSqlAccess.cs`

- [ ] **Step 1.1: Add `InsertRatingAsync` to the interface**

In `api/Ez.Hress.Hardhead/UseCases/IHardheadDataAccess.cs`, append before the closing brace of the interface (after `GetAverageRatingAsync` on line 38):

```csharp
    Task<int> InsertRatingAsync(int eventId, int userId, string typeCode, int rating);
```

- [ ] **Step 1.2: Implement in `HardheadSqlAccess`**

In `api/Ez.Hress.Hardhead/DataAccess/HardheadSqlAccess.cs`, add this method immediately after `GetAverageRatingAsync` (currently ends at line 608), before the class's closing brace:

```csharp
    public async Task<int> InsertRatingAsync(int eventId, int userId, string typeCode, int rating)
    {
        var sql = @"INSERT INTO [dbo].[rep_Count] ([EventId],[TypeId],[Count],[Inserted],[InsertedBy])
                    VALUES (@hardheadID, (SELECT t.ID FROM gen_Type t WHERE t.Shortcode = @ratingCode), @rating, GETDATE(), @userID)";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("hardheadID", eventId));
        command.Parameters.Add(new SqlParameter("userID", userId));
        command.Parameters.Add(new SqlParameter("ratingCode", typeCode));
        command.Parameters.Add(new SqlParameter("rating", rating));

        return await command.ExecuteNonQueryAsync();
    }
```

- [ ] **Step 1.3: Build the backend solution**

Run from `api/`:

```bash
cd api && dotnet build Ez.Hress.sln
```

Expected: build succeeds with no errors. (There may be pre-existing warnings; only new errors matter.)

- [ ] **Step 1.4: Commit**

```bash
git add api/Ez.Hress.Hardhead/UseCases/IHardheadDataAccess.cs api/Ez.Hress.Hardhead/DataAccess/HardheadSqlAccess.cs
git commit -m "Add: InsertRatingAsync on IHardheadDataAccess (#672)"
```

---

## Task 2: Add `UpdateRatingAsync` to data access

**Files:**
- Modify: `api/Ez.Hress.Hardhead/UseCases/IHardheadDataAccess.cs`
- Modify: `api/Ez.Hress.Hardhead/DataAccess/HardheadSqlAccess.cs`

- [ ] **Step 2.1: Add `UpdateRatingAsync` to the interface**

In `IHardheadDataAccess.cs`, append immediately after `InsertRatingAsync`:

```csharp
    Task<int> UpdateRatingAsync(int eventId, int userId, string typeCode, int rating);
```

- [ ] **Step 2.2: Implement in `HardheadSqlAccess`**

In `HardheadSqlAccess.cs`, add immediately after `InsertRatingAsync` (before the class's closing brace):

```csharp
    public async Task<int> UpdateRatingAsync(int eventId, int userId, string typeCode, int rating)
    {
        var sql = @"UPDATE [dbo].[rep_Count]
                       SET [Count] = @rating,
                           [Updated] = GETDATE(),
                           [UpdatedBy] = @userID
                     WHERE EventId = @eventID
                       AND InsertedBy = @userID
                       AND TypeId = (SELECT t.ID FROM gen_Type t WHERE t.Shortcode = @ratingCode)";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("eventID", eventId));
        command.Parameters.Add(new SqlParameter("userID", userId));
        command.Parameters.Add(new SqlParameter("ratingCode", typeCode));
        command.Parameters.Add(new SqlParameter("rating", rating));

        return await command.ExecuteNonQueryAsync();
    }
```

- [ ] **Step 2.3: Build**

```bash
cd api && dotnet build Ez.Hress.sln
```

Expected: build succeeds.

- [ ] **Step 2.4: Commit**

```bash
git add api/Ez.Hress.Hardhead/UseCases/IHardheadDataAccess.cs api/Ez.Hress.Hardhead/DataAccess/HardheadSqlAccess.cs
git commit -m "Add: UpdateRatingAsync on IHardheadDataAccess (#672)"
```

---

## Task 3: Test-drive `SaveRatingAsync` — update path

**Files:**
- Create: `api/Ez.Hress.UnitTest/Hardhead/HardheadInteractorTests.cs`

The test file does not exist yet; this task creates it with the first failing test, then adds the minimal implementation to pass it.

- [ ] **Step 3.1: Write the failing test**

Create `api/Ez.Hress.UnitTest/Hardhead/HardheadInteractorTests.cs` with:

```csharp
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Ez.Hress.UnitTest.Hardhead;

public class HardheadInteractorTests
{
    private readonly Mock<IHardheadDataAccess> _hardheadDataAccess;
    private readonly Mock<ILogger<HardheadInteractor>> _log;
    private readonly HardheadInteractor _interactor;

    public HardheadInteractorTests()
    {
        _hardheadDataAccess = new Mock<IHardheadDataAccess>();
        _log = new Mock<ILogger<HardheadInteractor>>();
        _interactor = new HardheadInteractor(_hardheadDataAccess.Object, _log.Object);
    }

    [Fact]
    public async Task SaveRatingAsync_ExistingRating_UpdatesAndReturnsTrue()
    {
        // ARRANGE
        const int eventId = 42;
        const int userId = 7;
        const string typeCode = "REP_C_RTNG";
        const int rating = 4;

        _hardheadDataAccess
            .Setup(d => d.GetMyRatingAsync(eventId, userId))
            .ReturnsAsync(new Dictionary<string, int> { { typeCode, 3 } });

        _hardheadDataAccess
            .Setup(d => d.UpdateRatingAsync(eventId, userId, typeCode, rating))
            .ReturnsAsync(1);

        // ACT
        var result = await _interactor.SaveRatingAsync(eventId, userId, typeCode, rating);

        // ASSERT
        Assert.True(result);
        _hardheadDataAccess.Verify(d => d.UpdateRatingAsync(eventId, userId, typeCode, rating), Times.Once);
        _hardheadDataAccess.Verify(d => d.InsertRatingAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        _hardheadDataAccess.Verify(d => d.GetGuests(It.IsAny<int>()), Times.Never);
    }
}
```

- [ ] **Step 3.2: Run the test — expect FAIL**

```bash
cd api && dotnet test Ez.Hress.UnitTest/Ez.Hress.UnitTest.csproj --filter "FullyQualifiedName~HardheadInteractorTests"
```

Expected: compilation error — `'HardheadInteractor' does not contain a definition for 'SaveRatingAsync'`. That is the failing test.

- [ ] **Step 3.3: Implement `SaveRatingAsync` (update branch only)**

In `api/Ez.Hress.Hardhead/UseCases/HardheadInteractor.cs`, append immediately before the class's closing brace (the file currently ends at line 227 with `}`):

```csharp
    public async Task<bool> SaveRatingAsync(int id, int userId, string typeCode, int rating)
    {
        var existing = await _hardheadDataAccess.GetMyRatingAsync(id, userId).ConfigureAwait(false);

        if (existing.ContainsKey(typeCode))
        {
            var affected = await _hardheadDataAccess.UpdateRatingAsync(id, userId, typeCode, rating).ConfigureAwait(false);
            return affected == 1;
        }

        return false;
    }
```

- [ ] **Step 3.4: Run the test — expect PASS**

```bash
cd api && dotnet test Ez.Hress.UnitTest/Ez.Hress.UnitTest.csproj --filter "FullyQualifiedName~SaveRatingAsync_ExistingRating"
```

Expected: 1 passing test.

- [ ] **Step 3.5: Commit**

```bash
git add api/Ez.Hress.UnitTest/Hardhead/HardheadInteractorTests.cs api/Ez.Hress.Hardhead/UseCases/HardheadInteractor.cs
git commit -m "Add: SaveRatingAsync update path + test (#672)"
```

---

## Task 4: Extend `SaveRatingAsync` — insert path (attended)

**Files:**
- Modify: `api/Ez.Hress.UnitTest/Hardhead/HardheadInteractorTests.cs`
- Modify: `api/Ez.Hress.Hardhead/UseCases/HardheadInteractor.cs`

- [ ] **Step 4.1: Add the failing test**

In `HardheadInteractorTests.cs`, ensure these `using` directives are at the top (both were added in Task 3 — verify they remain):

```csharp
using System.Collections.Generic;
using Ez.Hress.Shared.Entities;
```

Then add this `[Fact]` inside the class (after the existing test):

```csharp
    [Fact]
    public async Task SaveRatingAsync_NewRating_UserAttended_InsertsAndReturnsTrue()
    {
        // ARRANGE
        const int eventId = 42;
        const int userId = 7;
        const string typeCode = "REP_C_MRTNG";
        const int rating = 5;

        _hardheadDataAccess
            .Setup(d => d.GetMyRatingAsync(eventId, userId))
            .ReturnsAsync(new Dictionary<string, int>()); // no existing rating

        _hardheadDataAccess
            .Setup(d => d.GetGuests(eventId))
            .ReturnsAsync(new List<UserBasicEntity> { new() { ID = userId } });

        _hardheadDataAccess
            .Setup(d => d.InsertRatingAsync(eventId, userId, typeCode, rating))
            .ReturnsAsync(1);

        // ACT
        var result = await _interactor.SaveRatingAsync(eventId, userId, typeCode, rating);

        // ASSERT
        Assert.True(result);
        _hardheadDataAccess.Verify(d => d.InsertRatingAsync(eventId, userId, typeCode, rating), Times.Once);
        _hardheadDataAccess.Verify(d => d.UpdateRatingAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }
```

- [ ] **Step 4.2: Run — expect FAIL**

```bash
cd api && dotnet test Ez.Hress.UnitTest/Ez.Hress.UnitTest.csproj --filter "FullyQualifiedName~SaveRatingAsync_NewRating_UserAttended"
```

Expected: test fails — `Assert.True() Failure. Expected: True Actual: False` (current implementation returns `false` for the no-existing-rating path).

- [ ] **Step 4.3: Extend the implementation**

Replace the body of `SaveRatingAsync` in `api/Ez.Hress.Hardhead/UseCases/HardheadInteractor.cs` with:

```csharp
    public async Task<bool> SaveRatingAsync(int id, int userId, string typeCode, int rating)
    {
        var existing = await _hardheadDataAccess.GetMyRatingAsync(id, userId).ConfigureAwait(false);

        if (existing.ContainsKey(typeCode))
        {
            var updated = await _hardheadDataAccess.UpdateRatingAsync(id, userId, typeCode, rating).ConfigureAwait(false);
            return updated == 1;
        }

        var guests = await _hardheadDataAccess.GetGuests(id).ConfigureAwait(false);
        if (guests == null || !guests.Any(g => g.ID == userId))
            return false;

        var inserted = await _hardheadDataAccess.InsertRatingAsync(id, userId, typeCode, rating).ConfigureAwait(false);
        return inserted == 1;
    }
```

If `HardheadInteractor.cs` does not already `using System.Linq;`, the build will fail because of `.Any(...)`. Check the top of the file; if missing, add `using System.Linq;`.

- [ ] **Step 4.4: Run — expect PASS (both tests)**

```bash
cd api && dotnet test Ez.Hress.UnitTest/Ez.Hress.UnitTest.csproj --filter "FullyQualifiedName~HardheadInteractorTests"
```

Expected: 2 passing tests.

- [ ] **Step 4.5: Commit**

```bash
git add api/Ez.Hress.UnitTest/Hardhead/HardheadInteractorTests.cs api/Ez.Hress.Hardhead/UseCases/HardheadInteractor.cs
git commit -m "Add: SaveRatingAsync insert path for attendees (#672)"
```

---

## Task 5: `SaveRatingAsync` — reject non-attendees

**Files:**
- Modify: `api/Ez.Hress.UnitTest/Hardhead/HardheadInteractorTests.cs`

The implementation already handles this path (it returns `false`). This task just locks the behavior in with a test — no implementation change required.

- [ ] **Step 5.1: Add the test**

In `HardheadInteractorTests.cs`, add this `[Fact]` inside the class:

```csharp
    [Fact]
    public async Task SaveRatingAsync_NewRating_UserDidNotAttend_ReturnsFalse()
    {
        // ARRANGE
        const int eventId = 42;
        const int userId = 7;
        const string typeCode = "REP_C_RTNG";
        const int rating = 3;

        _hardheadDataAccess
            .Setup(d => d.GetMyRatingAsync(eventId, userId))
            .ReturnsAsync(new Dictionary<string, int>());

        _hardheadDataAccess
            .Setup(d => d.GetGuests(eventId))
            .ReturnsAsync(new List<UserBasicEntity>()); // user not in guest list

        // ACT
        var result = await _interactor.SaveRatingAsync(eventId, userId, typeCode, rating);

        // ASSERT
        Assert.False(result);
        _hardheadDataAccess.Verify(d => d.InsertRatingAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        _hardheadDataAccess.Verify(d => d.UpdateRatingAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }
```

- [ ] **Step 5.2: Run — expect PASS**

```bash
cd api && dotnet test Ez.Hress.UnitTest/Ez.Hress.UnitTest.csproj --filter "FullyQualifiedName~HardheadInteractorTests"
```

Expected: 3 passing tests.

- [ ] **Step 5.3: Commit**

```bash
git add api/Ez.Hress.UnitTest/Hardhead/HardheadInteractorTests.cs
git commit -m "Add: SaveRatingAsync test for non-attendee rejection (#672)"
```

---

## Task 6: `SaveRatingAsync` — input validation

**Files:**
- Modify: `api/Ez.Hress.UnitTest/Hardhead/HardheadInteractorTests.cs`
- Modify: `api/Ez.Hress.Hardhead/UseCases/HardheadInteractor.cs`

- [ ] **Step 6.1: Add a failing `[Theory]` for rating bounds and invalid typeCode**

In `HardheadInteractorTests.cs`, add:

```csharp
    [Theory]
    [InlineData(0, "REP_C_RTNG")]
    [InlineData(6, "REP_C_RTNG")]
    [InlineData(-1, "REP_C_RTNG")]
    [InlineData(3, "BOGUS_CODE")]
    [InlineData(3, "")]
    public async Task SaveRatingAsync_InvalidInput_ThrowsArgumentException(int rating, string typeCode)
    {
        // ACT & ASSERT
        await Assert.ThrowsAsync<System.ArgumentException>(
            () => _interactor.SaveRatingAsync(42, 7, typeCode, rating));
    }
```

- [ ] **Step 6.2: Run — expect FAIL**

```bash
cd api && dotnet test Ez.Hress.UnitTest/Ez.Hress.UnitTest.csproj --filter "FullyQualifiedName~SaveRatingAsync_InvalidInput"
```

Expected: theory cases fail — no `ArgumentException` is thrown (the current implementation either returns `false` or attempts a call).

- [ ] **Step 6.3: Add validation at the top of `SaveRatingAsync`**

Prepend the body of `SaveRatingAsync` in `HardheadInteractor.cs` with:

```csharp
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));

        if (typeCode != "REP_C_RTNG" && typeCode != "REP_C_MRTNG")
            throw new ArgumentException("Unknown rating type.", nameof(typeCode));
```

The full method now reads:

```csharp
    public async Task<bool> SaveRatingAsync(int id, int userId, string typeCode, int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));

        if (typeCode != "REP_C_RTNG" && typeCode != "REP_C_MRTNG")
            throw new ArgumentException("Unknown rating type.", nameof(typeCode));

        var existing = await _hardheadDataAccess.GetMyRatingAsync(id, userId).ConfigureAwait(false);

        if (existing.ContainsKey(typeCode))
        {
            var updated = await _hardheadDataAccess.UpdateRatingAsync(id, userId, typeCode, rating).ConfigureAwait(false);
            return updated == 1;
        }

        var guests = await _hardheadDataAccess.GetGuests(id).ConfigureAwait(false);
        if (guests == null || !guests.Any(g => g.ID == userId))
            return false;

        var inserted = await _hardheadDataAccess.InsertRatingAsync(id, userId, typeCode, rating).ConfigureAwait(false);
        return inserted == 1;
    }
```

- [ ] **Step 6.4: Run all interactor tests — expect PASS**

```bash
cd api && dotnet test Ez.Hress.UnitTest/Ez.Hress.UnitTest.csproj --filter "FullyQualifiedName~HardheadInteractorTests"
```

Expected: 8 passing tests (1 update + 1 insert + 1 reject + 5 theory cases).

- [ ] **Step 6.5: Commit**

```bash
git add api/Ez.Hress.UnitTest/Hardhead/HardheadInteractorTests.cs api/Ez.Hress.Hardhead/UseCases/HardheadInteractor.cs
git commit -m "Add: SaveRatingAsync input validation (#672)"
```

---

## Task 7: Wire the POST branch in `HardheadRatingFunctions`

**Files:**
- Modify: `api/Ez.Hress.FunctionsApi/Hardhead/HardheadRatingFunctions.cs`

- [ ] **Step 7.1: Add `using` declarations**

At the top of `api/Ez.Hress.FunctionsApi/Hardhead/HardheadRatingFunctions.cs`, add (if not already present):

```csharp
using System.IO;
using System.Text.Json;
```

- [ ] **Step 7.2: Add the request DTO at the top of the file**

After `namespace Ez.Hress.FunctionsApi.Hardhead;` and before `public class HardheadRatingFunctions`, add:

```csharp
public class RatingSaveRequest
{
    public string Type { get; set; } = string.Empty;
    public int Rating { get; set; }
}
```

(Matches the public-class-with-setters convention used by `CreateAlbumRequest` in `Albums/AlbumsFunction.cs`.)

- [ ] **Step 7.3: Replace the `NotImplementedException` with the POST handler**

In `HardheadRatingFunctions.cs`, replace lines 40–49 (currently `if (HttpMethods.IsPost(req.Method)) { ... throw new NotImplementedException(); }`) with:

```csharp
            if (HttpMethods.IsPost(req.Method))
            {
                if (!isJWTValid)
                {
                    _log.LogInformation("[{Class}.{Function}] JWT is not valid!", _class, nameof(Run));
                    return new UnauthorizedResult();
                }

                if (id <= 0)
                {
                    _log.LogInformation("[{Class}.{Function}] Invalid ID: {ID}", _class, nameof(Run), id);
                    return new BadRequestObjectResult("Invalid ID");
                }

                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonSerializer.Deserialize<RatingSaveRequest>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (request == null || string.IsNullOrWhiteSpace(request.Type))
                    return new BadRequestObjectResult("Missing type or rating");

                var ok = await _hardheadInteractor
                    .SaveRatingAsync(id, userID, request.Type, request.Rating)
                    .ConfigureAwait(false);

                return ok
                    ? new CreatedResult(string.Empty, null)
                    : new BadRequestObjectResult("Save failed");
            }
```

- [ ] **Step 7.4: Add a `catch (JsonException)` to the existing `try/catch`**

Below the existing `catch (ArgumentException aex)` block (around line 67), add:

```csharp
        catch (JsonException jex)
        {
            _log.LogError(jex, "[{Class}.{Method}] Invalid JSON body", _class, nameof(Run));
            return new BadRequestObjectResult("Invalid JSON body");
        }
```

- [ ] **Step 7.5: Build**

```bash
cd api && dotnet build Ez.Hress.sln
```

Expected: build succeeds.

- [ ] **Step 7.6: Run all backend tests (sanity)**

```bash
cd api && dotnet test Ez.Hress.UnitTest/Ez.Hress.UnitTest.csproj
```

Expected: all tests pass (no regression).

- [ ] **Step 7.7: Commit**

```bash
git add api/Ez.Hress.FunctionsApi/Hardhead/HardheadRatingFunctions.cs
git commit -m "Add: POST /hardhead/{id}/ratings handler (#672)"
```

---

## Task 8: Delete dead placeholder files

**Files:**
- Delete: `api/Ez.Hress.Hardhead/UseCases/IRatingDataAccess.cs`
- Delete: `api/Ez.Hress.Hardhead/UseCases/RatingInteractor.cs`

Both files are fully commented-out placeholders never wired into DI; with `SaveRatingAsync` now living on `HardheadInteractor`, they are confusing dead code.

- [ ] **Step 8.1: Delete the files**

```bash
git rm api/Ez.Hress.Hardhead/UseCases/IRatingDataAccess.cs api/Ez.Hress.Hardhead/UseCases/RatingInteractor.cs
```

- [ ] **Step 8.2: Build**

```bash
cd api && dotnet build Ez.Hress.sln
```

Expected: build succeeds.

- [ ] **Step 8.3: Commit**

```bash
git commit -m "Remove: dead IRatingDataAccess and RatingInteractor placeholders (#672)"
```

---

## Task 9: Add `saveRating` to `useRatings` hook

**Files:**
- Modify: `src/pages/hardhead/hooks/useRatings.ts`

- [ ] **Step 9.1: Replace the file contents**

Open `src/pages/hardhead/hooks/useRatings.ts`. Replace the entire file with:

```ts
import { useState, useEffect } from "react";
import axios from "axios";
import config from "react-global-configuration";
import { useAuth } from "../../../context/auth";
import { RatingsResponse } from "../../../types/ratings";

export const useRatings = (id: number) => {
  const { authTokens } = useAuth();
  const [ratings, setRatings] = useState<RatingsResponse>();

  const getRatingData = async () => {
    if (authTokens !== undefined) {
      try {
        const url = `${config.get("apiPath")}/api/hardhead/${id}/ratings`;
        const response = await axios.get<RatingsResponse>(url, {
          headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
        });
        setRatings(response.data);
      } catch (e) {
        console.error(e);
      }
    }
  };

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

  useEffect(() => {
    getRatingData();
  }, [id, authTokens]);

  return { ratings, refreshRatings: getRatingData, saveRating };
};
```

Notes:
- The GET path is unchanged from the existing file (verbatim) — only `saveRating` is added and the return object grows one key.
- `saveRating` does not swallow errors; the caller decides UX.
- After a successful POST, `getRatingData()` re-fetches so the UI reflects the new rating without an extra round-trip in the component.

- [ ] **Step 9.2: TypeScript check**

From the repo root:

```bash
npx tsc --noEmit
```

Expected: completes without errors.

- [ ] **Step 9.3: Commit**

```bash
git add src/pages/hardhead/hooks/useRatings.ts
git commit -m "Add: saveRating to useRatings hook (#672)"
```

---

## Task 10: Update `rating.tsx` to use the hook's `saveRating`

**Files:**
- Modify: `src/pages/hardhead/components/rating.tsx`

- [ ] **Step 10.1: Replace the file contents**

Open `src/pages/hardhead/components/rating.tsx`. Replace the entire file with:

```tsx
import { useState } from "react";
import { useAuth } from "../../../context/auth";
import { useRatings } from "../hooks/useRatings";
import { RatingEntity } from "../../../types/ratings";
import StarRating from "../../../components/StarRating";

interface HardheadRatingProps {
  id: number;
  nightRatingVisible: boolean;
  movieRatingVisible: boolean;
}

const HardheadRating = ({
  id,
  nightRatingVisible,
  movieRatingVisible,
}: HardheadRatingProps) => {
  const { authTokens } = useAuth();
  const { ratings, saveRating } = useRatings(id);
  const [hoverRatings, setHoverRatings] = useState<{ [key: string]: number }>(
    {}
  );

  const getRatingText = (rate: number | undefined, type: string) => {
    if (!rate) return "";
    rate = Math.round(rate);
    if (rate === 1)
      return type === "REP_C_RTNG" ? "dræmt kvöld " : "hræðileg mynd ";
    else if (rate === 2)
      return type === "REP_C_RTNG" ? "ágætt kvöld " : "slæm mynd ";
    else if (rate === 3)
      return type === "REP_C_RTNG" ? "gott kvöld " : "ágæt mynd ";
    else if (rate === 4)
      return type === "REP_C_RTNG" ? "hresst kvöld " : "góð mynd ";
    else if (rate === 5)
      return type === "REP_C_RTNG" ? "frábært kvöld " : "frábær mynd ";
    else return "";
  };

  const handleSaveRating = async (rate: number, type: string) => {
    try {
      await saveRating(rate, type);
    } catch (e) {
      console.error(e);
      alert("Ekki tókst að vista einkunn, reyndu aftur síðar.");
    }
  };

  const handleHoverChange = (rating: number, code: string) => {
    setHoverRatings((prev) => ({
      ...prev,
      [code]: rating,
    }));
  };

  return (
    <ul className="stats">
      {authTokens && authTokens ? null : (
        <li>Skráðu þig inn til þess að gefa einkunn</li>
      )}
      {ratings?.ratings?.map((rating: RatingEntity) =>
        (rating.code === "REP_C_RTNG" && nightRatingVisible === true) ||
        (rating.code === "REP_C_MRTNG" && movieRatingVisible === true) ? (
          <li key={rating.code}>
            <span id={`${rating.code}_${id}`} />
            {rating.code === "REP_C_RTNG" ? (
              <i className="icon solid fa-beer fa-2x" />
            ) : (
              <i className="icon solid fa-film fa-2x" />
            )}
            {ratings.readonly &&
            rating.myRating === undefined &&
            rating.averageRating === undefined ? null : (
              <>
                <StarRating
                  rating={
                    ratings.readonly
                      ? rating.averageRating ?? 0
                      : rating.myRating ?? 0
                  }
                  starRatedColor="gold"
                  starHoverColor="orange"
                  starEmptyColor="rgb(226, 226, 226)"
                  changeRating={(newRating: number) =>
                    handleSaveRating(newRating, rating.code)
                  }
                  numberOfStars={5}
                  starDimension="20px"
                  starSpacing="2px"
                  readonly={ratings.readonly}
                  onHoverChange={(hoverRating) =>
                    handleHoverChange(hoverRating, rating.code)
                  }
                />
                <div style={{ paddingTop: "5px" }}>
                  {getRatingText(
                    hoverRatings[rating.code] ||
                      (ratings.readonly
                        ? rating.averageRating
                        : rating.myRating),
                    rating.code
                  )}
                  {ratings.readonly && rating.averageRating
                    ? `(${Math.round(rating.averageRating * 10) / 10})`
                    : null}
                </div>
              </>
            )}
            {ratings.readonly ? (
              <div style={{ paddingTop: "5px" }}>
                ({rating.numberOfRatings ?? 0} atkvæði
                {rating.myRating ? ` -  Þitt ${rating.myRating}` : null})
              </div>
            ) : null}
          </li>
        ) : null
      )}
    </ul>
  );
};

export default HardheadRating;
```

Changes from the previous version:
- Removed imports: `axios`, `react-global-configuration`.
- Removed inline `saveRating` (old lines 42–65).
- `useRatings` destructure now pulls `saveRating` instead of `refreshRatings`.
- Added `handleSaveRating` that wraps the hook call with try/catch + alert.
- `StarRating.changeRating` now calls `handleSaveRating`.

- [ ] **Step 10.2: TypeScript check**

```bash
npx tsc --noEmit
```

Expected: completes without errors.

- [ ] **Step 10.3: Verify acceptance grep**

```bash
grep -nE 'config\.get\("path"\)|config\.get\("code"\)' src/pages/hardhead/components/rating.tsx
```

Expected: no output (zero matches).

- [ ] **Step 10.4: Commit**

```bash
git add src/pages/hardhead/components/rating.tsx
git commit -m "Migrate: rating save in rating.tsx to new API (#672)"
```

---

## Task 11: End-to-end smoke test

This task does not write code. It verifies the migration works against a running API + frontend, satisfying the acceptance criteria from the spec.

- [ ] **Step 11.1: Start the Azure Functions backend**

In one terminal, from `api/Ez.Hress.FunctionsApi/`:

```bash
cd api/Ez.Hress.FunctionsApi && func start
```

Expected: function host starts and lists `ratings: [GET,POST] http://localhost:7071/api/hardhead/{id:int}/ratings`.

If `func` (Azure Functions Core Tools) is not installed, install it per the existing project conventions (see `Hress.Org/api/README.md` if present, or run `brew tap azure/functions && brew install azure-functions-core-tools@4`).

- [ ] **Step 11.2: Start the React dev server**

In a second terminal, from the repo root:

```bash
npm start
```

Expected: dev server on `http://localhost:3000` (or whatever port `package.json` configures).

- [ ] **Step 11.3: Manual happy path — night rating**

1. Log in as a user who attended an upcoming-rateable hardhead night.
2. Navigate to that night's detail page.
3. Click a star on the night rating row.
4. Verify: no alert, the star fills, and a refresh of the page shows the same rating persisted.

- [ ] **Step 11.4: Manual happy path — movie rating**

Same as above against the movie rating row.

- [ ] **Step 11.5: Manual edge — re-rate (update path)**

Click a different star on the same row to change the rating. Verify it updates without an alert and persists across refresh.

- [ ] **Step 11.6: Manual edge — non-attendee**

Log in as a user who did not attend a recent night. Open that night. The UI should be readonly per existing `GetRatingAsync` rules; if for any reason the post is attempted, the backend returns 400 and the alert fires. (Optional smoke step; document the result.)

- [ ] **Step 11.7: Manual edge — anonymous**

Log out. Reload the night detail page. Rating UI should not be interactive. If a POST somehow fires, expect 401.

- [ ] **Step 11.8: Network-tab verification**

In the browser DevTools Network tab during a successful save, confirm:
- URL is `${apiPath}/api/hardhead/<id>/ratings` (no `?code=...`).
- Request header `X-Custom-Authorization: token <jwt>` is present.
- Response status is `201 Created`.

This task has no commit step — it's verification only.

---

## Acceptance Recap

Per `docs/superpowers/specs/2026-05-27-rating-save-migration-design.md`:

- [x] `saveRating` POSTs to `${apiPath}/api/hardhead/${id}/ratings` with `X-Custom-Authorization`. (Task 9, 10)
- [x] Save works for both `REP_C_RTNG` and `REP_C_MRTNG`, insert and update branches. (Tasks 1, 2, 3, 4, 11)
- [x] Non-attendee receives 400. (Tasks 5, 7)
- [x] Missing/invalid JWT receives 401. (Task 7)
- [x] No `config.get("path")` or `config.get("code")` in `rating.tsx`. (Task 10.3)
- [x] Unit tests for `SaveRatingAsync` pass. (Tasks 3–6)
- [x] `IRatingDataAccess.cs` and `RatingInteractor.cs` deleted. (Task 8)
