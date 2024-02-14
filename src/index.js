import {React, version} from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import * as serviceWorker from './serviceWorker';
import config from 'react-global-configuration';
import App from './pages/App';
import dotenv from 'dotenv';

dotenv.config();

function setConfig() {
  config.set({
    path: process.env.REACT_APP_LEGACY_API_PATH,
    apiPath: process.env.REACT_APP_API_PATH ?? window.location.hostname, 
    imagePath: process.env.REACT_APP_IMAGE_PATH,
    code: process.env.REACT_APP_LEGACY_API_CODE,
    omdb: process.env.REACT_APP_OMDB
  }, {
    freeze: false,
    environment: 'prod'
  });
  config.set({});

  config.setEnvironment("prod");
}
setConfig();

console.log("Hress version: " + process.env.REACT_APP_VERSION);

// ReactDOM.render(<App/>, document.getElementById('content'));
const container = document.getElementById("content");
const root = createRoot(container);
root.render(<App />);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
