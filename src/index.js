import React from 'react';
import {createRoot} from 'react-dom/client';
import './index.css';
import * as serviceWorker from './serviceWorker';
import config from 'react-global-configuration';
import App from './pages/App';

  function setConfig() {  
    config.set({ path: 'https://ezhressapi.azurewebsites.net', apiPath: '', imagePath: 'https://ezcontentapi.azurewebsites.net', code: 'JRXXeaXTE5Y9WD2kVAYLu6gXknrmLlluqfTJZfo3pZfo4kkBUzf3Yw==', omdb: '8ae68ed6' }, {freeze: false, environment: 'prod'});
    config.set({ path: 'https://ezhressapi.azurewebsites.net', apiPath: 'http://192.168.86.166:7072', imagePath: 'https://ezcontentapi.azurewebsites.net', code: 'JRXXeaXTE5Y9WD2kVAYLu6gXknrmLlluqfTJZfo3pZfo4kkBUzf3Yw==', omdb: '8ae68ed6'}, {freeze: false, environment: 'hybrid2'});
    config.set({ path: 'http://localhost:7071', apiPath: 'http://localhost:7072', imagePath: 'https://ezcontentapi.azurewebsites.net', code: '', omdb: '8ae68ed6'}, {freeze: false, environment: 'hybrid'});
    config.set({ path: 'http://localhost:7071', apiPath: 'http://localhost:7072', imagePath: 'https://localhost:44363', code: '', omdb: '8ae68ed6'}, {freeze: false, environment: 'dev'});
    config.set({ });
  
    config.setEnvironment("prod");
  }
  setConfig(); 

// ReactDOM.render(<App/>, document.getElementById('content'));
const container = document.getElementById("content");
const root = createRoot(container);
root.render(<App />);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
