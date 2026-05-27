import * as Sentry from "@sentry/react";
import { StrictMode, useEffect } from "react";
import { useLocation, useNavigationType, createRoutesFromChildren, matchRoutes } from "react-router-dom";
import { createRoot } from "react-dom/client";
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import "./index.css";
import * as serviceWorker from "./serviceWorker";
import config from "react-global-configuration";
import App from "./pages/App";
const packageJson = require("../package.json");

Sentry.init({
  dsn: process.env.REACT_APP_SENTRY_DSN,
  integrations: [
    Sentry.browserTracingIntegration(),
    Sentry.reactRouterV7BrowserTracingIntegration({
      useEffect,
      useLocation,
      useNavigationType,
      createRoutesFromChildren,
      matchRoutes,
    }),
    Sentry.replayIntegration(),
  ],
  tracesSampleRate: 1.0,
  tracePropagationTargets: ["localhost", /^https:\/\/hress\.org/],
  replaysSessionSampleRate: 0.1,
  replaysOnErrorSampleRate: 1.0,
  initialScope: {
    tags: { version: packageJson.version ?? process.env.REACT_APP_VERSION },
  },
});

function setConfig() {
  config.set(
    {
      apiPath: process.env.REACT_APP_API_PATH ?? window.location.hostname,
      imagePath: process.env.REACT_APP_IMAGE_PATH,
      omdb: process.env.REACT_APP_OMDB,
    },
    {
      freeze: false,
      environment: "prod",
    }
  );
  config.set({});

  config.setEnvironment("prod");
}
setConfig();

// skipcq: JS-0002
console.log(
  "Hress version: " + packageJson.version ?? process.env.REACT_APP_VERSION
);

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: false,
    },
  },
});

const container = document.getElementById("content");
const root = createRoot(container);
root.render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>
  </StrictMode>
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
