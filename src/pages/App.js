import React, { useState } from 'react';
import { BrowserRouter as Router, Route, Switch, Link } from 'react-router-dom';
import { CompatRoute, CompatRouter } from 'react-router-dom-v5-compat';
import jwt from 'jsonwebtoken';
import ReactGA from 'react-ga4';

import { AuthContext } from './../context/auth';
import Hardhead from './hardhead';
import HardheadSidebar from './hardhead/sidebar';
import Awards from './hardhead/awards';
import Rules from './hardhead/rules';
import Statistics from './hardhead/statistics';
import Login from './frame/login';
import Magic from './frame/magic';
import Menu from './frame/menu';
import HardheadEdit from './hardhead/hardheadEdit';
import Election from './hardhead/awards/election';
import AwardsSidebar from './hardhead/awards/awardsSidebar';
import AwardsByYear from './hardhead/awards/awardsByYear';
import AwardsByType from './hardhead/awards/awardByType';
import HHUsers from './hardhead/hhusers';
import HHUserSidebar from './hardhead/hhusers/hhUserSidebar';
import Nominations from './hardhead/awards/nominations';
import LegacyFrame from './frame/legacyFrame';
import LegacyRedirect from './frame/legacyRedirect';
import Navigation from './frame/navigation';
import News from './news';
import MainSidebar from './news/mainSidebar';
import SingleNews from './news/singleNews';
import { Helmet } from 'react-helmet';
import HistoryNews from './news/history';
import HistorySidebar from './news/historySidebar';
import DinnerParties from './dinnerparties';
import Election2022 from './dinnerparties/election2022';
import DinnerParty from './dinnerparties/dinnerparty';
import Profile from './profile';
import Albums from './albums';
import Album from './albums/album';
import RuleChange from './hardhead/rules/change';
import Password from './profile/password';

function App(props) {
  const [authTokens, setAuthTokens] = useState();
  const [data, setData] = useState({ showMenu: false });
  const TRACKING_ID = "G-Z6HJ4VPZTN";


  const setTokens = (data) => {
    if (data) {
      localStorage.setItem("tokens", JSON.stringify(data));
      var decodedToken = jwt.decode(data.token, { complete: true });
      localStorage.setItem("userID", decodedToken.payload.sub);
      setAuthTokens(data);
    } else {
      setAuthTokens();
      localStorage.removeItem("tokens");
    }
  }

  const setGoogleAnalytics = () => {
    ReactGA.initialize(TRACKING_ID);
    ReactGA.send("pageview");
  }

  const checkExistingTokens = () => {
    if (authTokens === undefined) {
      var existingTokens = JSON.parse(localStorage.getItem("tokens"));
      if (existingTokens !== undefined && existingTokens !== null) {

        if (existingTokens.token === null) {
          console.log("checkExistingTokens - Faulty! remove from local storage");
          localStorage.removeItem("tokens");
          return;
        }

        var decodedToken = jwt.decode(existingTokens.token, { complete: true });
        var dateNow = new Date();
        if ((decodedToken.payload.exp * 1000) < (dateNow.getTime() + 1)) {
          console.log("checkExistingTokens - Expired! remove from local storage");
          localStorage.removeItem("tokens");
        }
        else {
          console.log("checkExistingTokens - set auth tokens");
          localStorage.setItem("userID", decodedToken.payload.sub);
          setAuthTokens(existingTokens);
        }
      }
    }
  }

  setGoogleAnalytics();

  checkExistingTokens();

  const toggleMenu = () => {
    var visible = !data.showMenu;

    if (visible)
      setData({ showMenu: visible, class: "is-menu-visible" })
    else
      setData({ showMenu: visible, class: "" })
  }

  return (
    <AuthContext.Provider value={{ authTokens, setAuthTokens: setTokens }}>
      <Helmet>
        <meta property="og:title" key="og:title" content="Hress.Org" />
        <meta property="og:url" key="og:url" content={window.location.href} />
      </Helmet>
      <div className={data.class}>
        <Router>
          <CompatRouter>
            <div id="wrapper" >
              <header id="header">
                <h1><Link to="/">Hress.Org</Link></h1>
                <Navigation />
                <nav className="main">
                  <ul>
                    <li className="search" onClick={() => toggleMenu(true)}>
                      <a className="fa-search" href="#search">Search</a>
                      {/* <form id="search" method="get" action="#">
                    <input type="text" name="query" placeholder="Search" />
                  </form> */}
                    </li>
                    <li className="menu" onClick={() => toggleMenu(true)}>
                      <a className="fa-bars" href="#menu">Menu</a>
                    </li>
                  </ul>
                </nav>
              </header>

              {/* Main section */}
              <Switch>
                <CompatRoute exact path="/" component={News} />
                <CompatRoute exact path="/hardhead/films.aspx" component={LegacyFrame} />
                <CompatRoute exact path="/Hardhead/defaultold.aspx" component={LegacyFrame} />
                <CompatRoute exact path="/hardhead" component={Hardhead} />
                <CompatRoute exact path="/hardhead/awards" component={Awards} />
                <CompatRoute path="/hardhead/awards/year/:id" component={AwardsByYear} />
                <CompatRoute exact path="/hardhead/awards/nominations" component={Nominations} />
                <CompatRoute exact path="/hardhead/awards/election" component={Election} />
                <CompatRoute path="/hardhead/awards/:id" component={AwardsByType} />
                <CompatRoute exact path="/hardhead/rules" component={Rules} />
                <CompatRoute path="/hardhead/rules/change" component={RuleChange} />
                <CompatRoute path="/hardhead/users/:id" component={HHUsers} />
                <CompatRoute path="/hardhead/stats" component={Statistics} />                
                <CompatRoute path="/hardhead/:hardheadID" component={HardheadEdit} />                                
                <CompatRoute exact path="/login" component={Login} />
                <CompatRoute path="/login/magic" component={Magic} />
                <CompatRoute path="/album/:id" component={Album} />
                <CompatRoute path="/album" component={Albums} />
                <CompatRoute path="/chat" component={LegacyFrame} />
                <CompatRoute path="/comic" component={LegacyFrame} />
                <CompatRoute path="/default/single.aspx" component={LegacyRedirect} />
                <CompatRoute path="/default" component={LegacyFrame} />
                <CompatRoute path="/feed" component={LegacyFrame} />
                <CompatRoute path="/foodandredwine" component={LegacyFrame} />
                {/* <Route path="/dinnerparties/courses/:typeID" component={Election2022} /> */}
                <CompatRoute path="/dinnerparties/:id" component={DinnerParty} />
                <CompatRoute path="/dinnerparties" component={DinnerParties} />
                <CompatRoute path="/gang" component={LegacyFrame} />
                <CompatRoute path="/hressgames" component={LegacyFrame} />
                <CompatRoute path="/mission" component={LegacyFrame} />
                <CompatRoute path="/news/history" component={HistoryNews} />
                <CompatRoute path="/news/:id" component={SingleNews} />
                <CompatRoute exact path="/profile" component={Profile} />
                <CompatRoute path="/profile/password" component={Password} />
                <CompatRoute path="/rss" component={LegacyFrame} />
                <CompatRoute path="/yearly" component={LegacyFrame} />
              </Switch>

              {/* Sidebar */}
              <Switch>
                <CompatRoute exact path="/" component={MainSidebar} />
                <CompatRoute exact path="/hardhead" component={HardheadSidebar} />
                <CompatRoute exact path="/hardhead/awards" component={AwardsSidebar} />
                <CompatRoute path="/hardhead/users/:id" component={HHUserSidebar} />
                <CompatRoute path="/news/history" component={HistorySidebar} />
              </Switch>
            </div>
            <Menu visible={data.showMenu} onClick={() => toggleMenu(true)} />
          </CompatRouter>
        </Router>
      </div>
    </AuthContext.Provider>
  );
}

export default App;
