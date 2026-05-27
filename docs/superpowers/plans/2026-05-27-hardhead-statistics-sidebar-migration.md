# Hardhead Statistics Sidebar Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrate `statisticsSide.js` off the legacy API (`config.get("path")` + `code` param) onto the new Azure Functions API (`config.get("apiPath")`), matching all peer files in the same directory.

**Architecture:** Single-file change. Replace the minute-based branching and legacy URL construction inside `useEffect` with `Math.random()` + `URLSearchParams`. Update all response property reads from PascalCase to camelCase and add a null-guard on the profile photo.

**Tech Stack:** React (functional component, hooks), axios, react-global-configuration.

---

### Task 1: Update `statisticsSide.js`

**Files:**
- Modify: `src/pages/hardhead/statistics/statisticsSide.js`

This is the only file that changes. No new files, no peer files, no API changes.

#### Context

Current file (`src/pages/hardhead/statistics/statisticsSide.js`) constructs two different legacy URLs using `config.get("path")` and appends `&code=${config.get("code")}`. Both branches hit `ezhressapi.azurewebsites.net`.

The new endpoint is `GET ${config.get("apiPath")}/api/hardhead/statistics/users` with optional query params `periodType` (int 0–4, maps to `All/Last10/Last5/Last2/ThisYear`) and `attendanceType` (int; omit or 0 → server defaults to 52 "gestur"; pass 53 for host stats). No `code` param — the Azure Function uses function-level auth automatically.

The new API returns camelCase JSON. The existing render block reads PascalCase (`data.stats.List`, `data.stats.PeriodTypeName`, etc.) — all must be updated.

- [ ] **Step 1: Replace the entire file content**

Open `src/pages/hardhead/statistics/statisticsSide.js` and replace it with:

```js
import { useState, useEffect } from "react";
import config from "react-global-configuration";
import { MiniPost } from "../../../components";
import axios from "axios";

const StatisticsSide = () => {
  const [data, setData] = useState({
    stats: null,
    isLoading: false,
    visible: false,
  });

  useEffect(() => {
    const getAwards = async () => {
      try {
        const isHost = Math.random() < 0.5;
        const periodType = Math.round(Math.random() * 4);
        const params = new URLSearchParams({ periodType });
        if (isHost) params.set("attendanceType", "53");
        const url = `${config.get("apiPath")}/api/hardhead/statistics/users?${params}`;

        setData({ isLoading: true });
        const response = await axios.get(url);
        setData({ stats: response.data, isLoading: false, visible: true });
      } catch (e) {
        console.error(e);
        setData({ isLoading: false, visible: false });
      }
    };

    if (!data.stats) {
      getAwards();
    }
  }, []);

  const getDescription = (period, guest) => {
    let description = "gestur";

    if (guest === "gestur") description = "Oftast mætt";
    else description = "Oftast haldið";

    if (period === "All") description = `${description} frá upphafi`;
    else if (period === "Last10")
      description = `${description} síðustu 10 árin`;
    else if (period === "Last5") description = `${description} síðustu 5 árin`;
    else if (period === "Last2") description = `${description} síðustu 2 árin`;
    else if (period === "ThisYear") description = `${description}  á þessu ári`;

    return description;
  };

  const top = data.stats?.list?.[0] ?? null;

  return (
    <div>
      {data.visible && top ? (
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
          userPhoto={
            top.user.profilePhoto
              ? config.get("apiPath") + top.user.profilePhoto.href
              : undefined
          }
          userText={top.user.username}
        />
      ) : null}
    </div>
  );
};

export default StatisticsSide;
```

Key changes from the original:
- `config.get("path")` → `config.get("apiPath")` (both branches)
- `&code=${config.get("code")}` removed
- `new Date().getMinutes() % 2 === 0` → `Math.random() < 0.5`
- `guestType=53` → `attendanceType=53`
- Single `periodType` random value replaces two separate ones
- All response property accesses updated to camelCase (`List` → `list`, `PeriodTypeName` → `periodTypeName`, `TypeName` → `typeName`, `DateFrom` → `dateFrom`, `DateFromString` → `dateFromString`, `User.Username` → `user.username`, `User.ID` → `user.id`, `User.ProfilePhoto.Href` → `user.profilePhoto.href`)
- `top.user.profilePhoto` null guard added (server returns `null` when user has no photo)

- [ ] **Step 2: Verify no legacy config references remain in this file**

Run:
```bash
grep -n 'config\.get("path")\|config\.get("code")' src/pages/hardhead/statistics/statisticsSide.js
```

Expected: no output (zero matches).

- [ ] **Step 3: Start the dev server and verify the sidebar loads**

```bash
npm start
```

Navigate to `http://localhost:3000/hardhead` in the browser. The statistics sidebar (`MiniPost` titled "Tölfræði") should appear. Open DevTools → Network tab, filter for `statistics/users`, and confirm:

- The request URL starts with `apiPath` (the Azure Functions base URL, e.g. `https://ezhressapi2.azurewebsites.net` or similar from your `.env`), **not** `ezhressapi.azurewebsites.net`.
- The request URL contains `periodType=` but no `code=` parameter.
- On the "host" branch (one in two reloads, roughly) the URL also contains `attendanceType=53`.
- The widget renders a username, attended count, and two date strings without a JavaScript error.

Reload the page 6–10 times to exercise both branches. Check the browser console for errors after each reload.

- [ ] **Step 4: Commit**

```bash
git add src/pages/hardhead/statistics/statisticsSide.js
git commit -m "Migrate: statisticsSide to new API (#673)

Replace legacy config.get(\"path\") + code param with config.get(\"apiPath\").
Update guestType→attendanceType, minute-branch→Math.random(), PascalCase→camelCase,
and add null-guard on profilePhoto."
```
