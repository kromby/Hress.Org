# AGENTS.md

Guidance for AI agents working in the Hress.Org repository. For human-facing setup, see [README.md](README.md). For backend conventions, see [ARCHITECTURE_RULES.md](ARCHITECTURE_RULES.md).

## What this is

Personal website at [www.hress.org](https://www.hress.org), hosted on **Azure Static Web Apps**.

- **Frontend** (`src/`) — React 19 SPA, built with Create React App via `react-app-rewired`. Deployed as the SWA static site.
- **API** (`api/`) — Azure Functions (.NET 9, isolated worker). Deployed as the SWA managed API.
- **Storage** — Azure SQL Database + Azure Blob/Table Storage.

The two halves are separate stacks in one repo. Decide which one a task touches before starting.

## Commands

### Frontend (repo root)

```bash
npm install
npm start        # dev server at http://localhost:3000
npm run dbuild   # production build (no Sentry sourcemap upload)
npm run build    # production build + Sentry sourcemap upload (CI use)
npm test         # Jest via react-app-rewired
```

Use `npm run dbuild` for local build checks — plain `npm run build` uploads sourcemaps to Sentry and needs `SENTRY_AUTH_TOKEN`.

### API (`api/Ez.Hress.FunctionsApi`)

```bash
cd api/Ez.Hress.FunctionsApi
func start       # API at http://localhost:7072 (needs Azure Functions Core Tools v4)
dotnet build api/Ez.Hress.FunctionsApi.sln
dotnet test                                   # runs Ez.Hress.UnitTest
```

The frontend expects the API at `http://localhost:7072` — set `REACT_APP_API_PATH=http://localhost:7072` in the root `.env`.

## Frontend architecture (`src/`)

- **Mixed JS/TS** — files are `.js`, `.jsx`, `.tsx` side by side. `allowJs` is on; `tsconfig.json` is `strict`. New code should prefer TypeScript (`.tsx`/`.ts`). Match the conventions of the file you're editing.
- **Data fetching** — `@tanstack/react-query`. Hooks live in `src/hooks/` (e.g. `useUsers.ts`, `useAlbums.ts`) and `src/hooks/hardhead/`. Add new data access as a query hook, don't fetch inline.
- **Routing** — `react-router-dom` v7. Pages live in `src/pages/<feature>/`.
- **Config** — `react-global-configuration`, set once in `src/index.js` from `REACT_APP_*` env vars (`apiPath`, `imagePath`, `omdb`). Read it via `config.get(...)`, don't read `process.env` deep in the tree.
- **Auth** — `AuthContext` and the `useAuth()` hook are defined in `src/context/auth.js`. The provider is wired directly in `src/pages/App.js` via `<AuthContext.Provider value={{ authTokens, setAuthTokens }}>` (token state held in `App.js`, no separate provider component). Consume auth with `useAuth()`. Protected routes use `src/components/access/privateRoute.js`. Note: `src/context/authProvider.js` is empty and `authConfig.js`/`graph.js` are commented-out legacy MSAL/MS Graph — dead code, don't build on them.
- **Errors/monitoring** — Sentry is initialized in `src/index.js`.
- **Webpack** — `config-overrides.js` injects Node polyfills (crypto, stream, buffer, etc.) for browser and removes `fork-ts-checker-webpack-plugin`. Don't reintroduce that plugin; TS checking runs via `tsc`.
- **Formatting** — Prettier (`.prettierrc`): 2-space indent, double quotes, semicolons, ES5 trailing commas.

## API architecture (`api/`)

Clean-architecture layout. Each domain is its own project; all follow the same three-folder split:

- `Entities/` — pure domain objects, no storage knowledge.
- `UseCases/` — interactors (no interfaces) + data-access interfaces (`I{Entity}DataAccess`).
- `DataAccess/` — `{Entity}TableAccess` / `{Entity}SqlAccess` implementations and `{Entity}TableEntity` (`ITableEntity`) classes.

Projects: `Ez.Hress.Shared`, `Ez.Hress.Hardhead`, `Ez.Hress.Album`, `Ez.Hress.MajorEvents`, `Ez.Hress.Administration`, `Ez.Hress.UserProfile`, `Ez.Hress.Scripts`. The HTTP-trigger functions and DI wiring live in `Ez.Hress.FunctionsApi` (`Program.cs`); tests live in `Ez.Hress.UnitTest`.

Key rules (full detail in [ARCHITECTURE_RULES.md](ARCHITECTURE_RULES.md)):

- Register everything in `Program.cs` with `services.AddSingleton<TInterface, TImpl>()`; use cases have no interfaces.
- Data-access classes take `BlobConnectionInfo` and `ILogger<T>` in their constructors.
- Async/await throughout; methods return `Task<T>`.
- Structured logging: `_log.LogInformation("[{Class}.{Method}] ...", nameof(Type), nameof(Method), ...)`.
- Never log secrets/connection strings.

When adding API features, add the test in `Ez.Hress.UnitTest` mirroring the domain folder.

## Local config

- Frontend: root `.env` with `REACT_APP_API_PATH`, `REACT_APP_IMAGE_PATH`, `REACT_APP_OMDB` (see README).
- API: `api/Ez.Hress.FunctionsApi/local.settings.json` (keys already present, fill values).
- The `_http/` folder holds VS Code REST Client request files for hitting the API.

Neither `.env` nor `local.settings.json` is committed — never add secrets to tracked files.

## Deployment & versioning

- Push to `main` triggers `.github/workflows/azure-static-web-apps-*.yml`, which builds the React app and publishes the Functions API to Azure SWA. The workflow writes its own `.env` from GitHub secrets.
- Version bumps use `npm version`; `postversion` auto-runs `git push && git push --tags`.

## Workflow notes

- Work happens on feature branches merged to `main` via PR (see git history; PRs are squash/numbered like `(#674)`).
- Planning docs and specs live under `docs/superpowers/plans/` and `docs/superpowers/specs/`.
- DeepSource runs static analysis (`.deepsource.toml`); suppress findings inline with `// skipcq: <rule>` only when justified.
