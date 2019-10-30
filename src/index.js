import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter as Router, Route, Switch } from 'react-router-dom';
import './index.css';
import * as serviceWorker from './serviceWorker';

import App from './App.js';
import UsersStatistics from './components/hardhead/userstats'
import SideMenu from './components/sidemenu';

// const appInsights = require("applicationinsights");
// appInsights.setup("aea8e204-ca43-49b0-8b92-489055e8074d");
// appInsights.start();

const routing = (
    <Router>
      <div>
          {/* <header id="header" className="alt">
              <h1 id="logo"><a href="index.html">boardgame.hress.org</a></h1>
              <nav id="nav">
                  <Link to="/">Leikur</Link>
                  -
                  <Link to="/board">Borð</Link>
              </nav>  
          </header> */}
  
        <Switch>
            <Route exact path="/" component={SideMenu} />
            <Route path="/userstats" component={UsersStatistics} />
            <Route component={App} />
        </Switch>
      </div>
    </Router>
  )  

ReactDOM.render(routing, document.getElementById('root'));

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
