# Hardhead Statistics Sidebar Migration — Legacy API → New API

**Issue:** [#673](https://github.com/kromby/Hress.Org/issues/673)
**Date:** 2026-05-27
**Status:** Approved (design)

## Background

The site is migrating off the legacy Azure Web API (`REACT_APP_LEGACY_API_PATH` → `ezhressapi.azurewebsites.net`) onto the new Azure Functions API in `Hress.Org/api/` (`Ez.Hress.FunctionsApi`).

The hardhead statistics sidebar (`src/pages/hardhead/statistics/statisticsSide.js`) still hits the legacy API on both of its random branches. Peer files in the same directory — `hostStats.js`, `guestStats.js`, `attendanceStats.js`, `actorStats.js` — already use `config.get("apiPath")`. This one was missed.

The new endpoint is fully implemented and in use by the peer files:

- `HardheadStatisticFunctions.RunUsers` (`api/Ez.Hress.FunctionsApi/Hardhead/HardheadStatisticFunctions.cs:64`) → `HardheadStatisticsInteractor.GetUserStatisticsAsync` → `HardheadStatisticSqlDataAccess.GetUserStatistic`.
- Route: `GET hardhead/statistics/users/{id:int?}`, Function-level auth.
- Query params: `periodType` (enum: `All`, `Last10`, `Last5`, `Last2`, `ThisYear`) and `attendanceType` (int; defaults to 52 — "gestur" — when 0 or omitted).

## Goal

The sidebar widget loads stats from the new API on both random branches, with no remaining references to `config.get("path")` or `config.get("code")` in the file.

## Non-goals

- Refactoring peer statistics files (`hostStats.js`, `guestStats.js`, etc.).
- Extracting a shared `useHardheadUserStats` hook.
- Restructuring the file to put `const url = ...` at the top level (already considered and rejected — current useEffect-local structure stays).
- Changes to the new API or its data access layer.
- Adding tests (no existing tests for this file or its peers).

## Design

### 1. URL construction

Inside `useEffect`, compute the random pick once and build the URL via `URLSearchParams`:

```js
const isHost = Math.random() < 0.5;
const periodType = Math.round(Math.random() * 4); // 0..4 maps to PeriodType enum positions
const params = new URLSearchParams({ periodType });
if (isHost) params.set("attendanceType", "53");
const url = `${config.get("apiPath")}/api/hardhead/statistics/users?${params}`;
```

Changes vs. today:

- `config.get("path")` → `config.get("apiPath")`.
- `&code=${config.get("code")}` dropped (new API uses Function-level auth, like peer files).
- `guestType=53` → `attendanceType=53` (the new API does not read `guestType`; peer `hostStats.js` already uses `attendanceType=53`). When `isHost` is false, the parameter is omitted, defaulting server-side to `attendanceType=52` ("gestur").
- Minute-based branching (`new Date().getMinutes() % 2 === 0`) replaced with `Math.random() < 0.5` — confirmed during brainstorming as the intended semantics. Two computed `periodType` values (one per branch) collapse to a single random value used by both branches.

### 2. Render block (camelCase + null-safe photo)

The new API returns camelCase JSON. Update all property reads, and guard the profile photo against `null` (the existing code crashes if the top user has no photo — `UserBasicEntity.ProfilePhoto` returns null when `ProfilePhotoId == 0`):

```jsx
const top = data.stats.list[0];

<MiniPost
  title="Tölfræði"
  href="/hardhead/stats"
  description={
    <span>
      {getDescription(data.stats.periodTypeName, data.stats.typeName)}
      <br />
      {top.user.username} - {top.attendedCount}
      <br />
      {top.firstAttendedString} - {top.lastAttendedString}
    </span>
  }
  date={data.stats.dateFrom}
  dateString={data.stats.dateFromString}
  userHref={`/hardhead/users/${top.user.id}`}
  userPhoto={top.user.profilePhoto ? config.get("apiPath") + top.user.profilePhoto.href : undefined}
  userText={top.user.username}
/>
```

Property mapping (legacy PascalCase → new camelCase):

| Legacy | New |
|---|---|
| `data.stats.PeriodTypeName` | `data.stats.periodTypeName` |
| `data.stats.TypeName` | `data.stats.typeName` |
| `data.stats.DateFrom` | `data.stats.dateFrom` |
| `data.stats.DateFromString` | `data.stats.dateFromString` |
| `data.stats.List[0]` | `data.stats.list[0]` |
| `.AttendedCount` | `.attendedCount` |
| `.FirstAttendedString` / `.LastAttendedString` | `.firstAttendedString` / `.lastAttendedString` |
| `.User.ID` / `.User.Username` | `.user.id` / `.user.username` |
| `.User.ProfilePhoto.Href` | `.user.profilePhoto.href` (nullable) |

`typeName` continues to compare against `"gestur"` in `getDescription` — the interactor sets it to `attendanceType.Name.ToLower()`, which is `"gestur"` for type 52 (the default branch).

### 3. Files changed

Single file: `src/pages/hardhead/statistics/statisticsSide.js`. No new files, no shared changes, no peer changes, no test changes.

## Testing

Manual verification only (no automated tests exist for this file or its peers):

- Load any page that mounts the sidebar widget; reload several times to exercise both random branches.
- Confirm in network tab that both branches hit `apiPath` (Azure Functions), not `ezhressapi.azurewebsites.net`.
- Confirm no `&code=` query parameter is present on the request URL.
- Confirm the rendered widget shows username, attended count, and date strings correctly for both `attendanceType=52` (no param, "Oftast mætt") and `attendanceType=53` ("Oftast haldið") branches.
- Confirm the sidebar does not crash when the top user has no profile photo (manually exercise by picking a `periodType` whose top result lacks a photo, if one exists in the data).

## Acceptance

From the issue:

- The sidebar widget loads stats from the new API.
- Both random branches (with and without `attendanceType=53`) work.
- No remaining references to `config.get("path")` or `config.get("code")` in this file.
