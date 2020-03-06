import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter as Router, Route, Switch, Link } from 'react-router-dom';
import './index.css';
import * as serviceWorker from './serviceWorker';
import config from 'react-global-configuration';

import App from './pages/App.js';
import Hardhead from './pages/hardhead';
import HardheadSidebar from './pages/hardhead/sidebar';
import UsersStatistics from './pages/hardhead/components/userstats'
import SideMenu from './components/sidemenu';

// const appInsights = require("applicationinsights");
// appInsights.setup("aea8e204-ca43-49b0-8b92-489055e8074d");
// appInsights.start();

const routing = (
    <Router>
      <div id="wrapper">
				<header id="header">
          <h1><a href="http://www.hress.org" target="_parent">Hress.Org</a></h1>
          <nav className="links">
            <ul>
              <li><Link to="/hardhead">Harðhaus</Link></li>
              <li><a href="http://www.hress.org/yearly" target="_parent">Árlegt</a></li>
              <li><a href="http://www.hress.org/links" target="_parent">Tenglar</a></li>
              <li><a href="http://www.hress.org/comic" target="_parent">Comic</a></li>
            </ul>
          </nav>
          {/* <nav className="main">
            <ul>
              <li className="search">
                <a className="fa-search" href="#search">Search</a>
                <form id="search" method="get" action="#">
                  <input type="text" name="query" placeholder="Search" />
                </form>
              </li>
              <li className="menu">
                <a className="fa-bars" href="#menu">Menu</a>
              </li>
            </ul>
          </nav> */}
        </header>

        {/* <section id="menu">
            <section>
              <form className="search" method="get" action="#">
                <input type="text" name="query" placeholder="Search" />
              </form>
            </section>

            <section>
              <ul className="links"> */}
                {/* <li>
                  <a href="#">
                    <h3>Lorem ipsum</h3>
                    <p>Feugiat tempus veroeros dolor</p>
                  </a>
                </li>
                <li>
                  <a href="#">
                    <h3>Dolor sit amet</h3>
                    <p>Sed vitae justo condimentum</p>
                  </a>
                </li>
                <li>
                  <a href="#">
                    <h3>Feugiat veroeros</h3>
                    <p>Phasellus sed ultricies mi congue</p>
                  </a>
                </li>
                <li>
                  <a href="#">
                    <h3>Etiam sed consequat</h3>
                    <p>Porta lectus amet ultricies</p>
                  </a>
                </li> */}
              {/* </ul>
            </section>

            <section>
              <ul className="actions stacked"> */}
                {/* <li><a href="#" className="button large fit">Log In</a></li> */}
              {/* </ul>
            </section>

					</section>           */}

          {/* Main section */}
          <Switch>
            <Route exact path="/" component={SideMenu} />
            <Route path="/userstats" component={UsersStatistics} />
            <Route path="/hardhead" component={Hardhead} />
            <Route component={App} />
          </Switch>
          
          {/* Sidebar */}
          <Switch>
            <Route exact path="/" component={SideMenu} />
            <Route path="/userstats" component={UsersStatistics} />
            <Route path="/hardhead" component={HardheadSidebar} />
            <Route component={App} />
          </Switch>          
      </div>
    </Router>
  )  

  function setConfig() {
    console.debug('setConfig');
  
    config.set({ path: 'https://ezhressapi.azurewebsites.net', code: 'JRXXeaXTE5Y9WD2kVAYLu6gXknrmLlluqfTJZfo3pZfo4kkBUzf3Yw==' }, {freeze: false, environment: 'prod'});
    config.set({ path: 'http://localhost:7071', code: ''}, {freeze: false, environment: 'dev'});
    config.set({ });
  
    config.setEnvironment('prod');
  }
    
  setConfig();  

ReactDOM.render(routing, document.getElementById('root'));

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
