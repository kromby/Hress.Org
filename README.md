[![DeepSource](https://app.deepsource.com/gh/kromby/Hress.Org.svg/?label=active+issues&show_trend=true&token=VdBUWHkYb47h94MHDxuGLAuZ)](https://app.deepsource.com/gh/kromby/Hress.Org/)

# Hress.org

Personal website running at [www.hress.org](https://www.hress.org), hosted on Azure Static Web Apps.

## Architecture

- **Frontend** — React 19, deployed as the SWA static site
- **API** — Azure Functions (.NET 9, isolated worker), deployed as the SWA managed API
- **Storage** — Azure SQL Database + Azure Blob/Table Storage

## Local Development

### Prerequisites

- Node.js
- .NET 9 SDK
- [Azure Functions Core Tools v4](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)

### Frontend

Create a `.env` file in the repo root:

```
REACT_APP_API_PATH=http://localhost:7072

REACT_APP_LEGACY_API_PATH=https://ezhressapi.azurewebsites.net
REACT_APP_LEGACY_API_CODE=ADD CODE HERE

REACT_APP_IMAGE_PATH=https://ezcontentapi.azurewebsites.net

REACT_APP_OMDB=ADD KEY HERE
```

Then run:

```bash
npm install
npm start        # dev server at http://localhost:3000
npm run build    # production build
```

### API

Fill in `api/Ez.Hress.FunctionsApi/local.settings.json` with the required values (see keys already present in the file), then run:

```bash
cd api/Ez.Hress.FunctionsApi
func start       # API at http://localhost:7072
```

## REST Client

The `_http` folder contains request files for the [VS Code REST Client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client).

Add this to VS Code `settings.json` to configure environments:

```json
"rest-client.environmentVariables": {
    "local": {
        "host": "http://localhost:7072",
        "apiCode": ""
    },
    "production": {
        "host": "https://www.hress.org",
        "apiCode": "ADD PROD CODE HERE"
    }
}
```

## Deployment

Deployment is automatic on push to `main` via the GitHub Actions workflow in `.github/workflows/`. The workflow builds the React app and publishes the Functions API to Azure Static Web Apps.
