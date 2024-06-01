import * as Sentry from "@sentry/react";
import React, { StrictMode } from "react";
import {
  useLocation,
  useNavigationType,
  createRoutesFromChildren,
  matchRoutes,
} from "react-router-dom";
import { createRoot } from "react-dom/client";
import "./index.css";
import * as serviceWorker from "./serviceWorker";
import config from "react-global-configuration";
import App from "./pages/App";
import dotenv from "dotenv";
const packageJson = require("../package.json");

Sentry.init({
  dsn: process.env.REACT_APP_SENTRY_DSN,
  integrations: [
    Sentry.reactRouterV6BrowserTracingIntegration({
      useEffect: React.useEffect,
      useLocation,
      useNavigationType,
      createRoutesFromChildren,
      matchRoutes,
    }),
    Sentry.replayIntegration(),
  ],
  // Performance Monitoring
  tracesSampleRate: 1.0, //  Capture 100% of the transactions
  // Set 'tracePropagationTargets' to control for which URLs distributed tracing should be enabled
  tracePropagationTargets: ["localhost", /^https:\/\/hress\.org/],
  // Session Replay
  replaysSessionSampleRate: 0.1, // This sets the sample rate at 10%. You may want to change it to 100% while in development and then sample at a lower rate in production.
  replaysOnErrorSampleRate: 1.0, // If you're not already sampling the entire session, change the sample rate to 100% when sampling sessions where errors occur.
  initialScope: {
    tags: { version: packageJson.version ?? process.env.REACT_APP_VERSION },
  },
});

dotenv.config();

function setConfig() {
  config.set(
    {
      path: process.env.REACT_APP_LEGACY_API_PATH,
      apiPath: process.env.REACT_APP_API_PATH ?? window.location.hostname,
      imagePath: process.env.REACT_APP_IMAGE_PATH,
      code: process.env.REACT_APP_LEGACY_API_CODE,
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

// ReactDOM.render(<App/>, document.getElementById('content'));
const container = document.getElementById("content");
const root = createRoot(container);
root.render(
  <StrictMode>
    <App />
  </StrictMode>
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
