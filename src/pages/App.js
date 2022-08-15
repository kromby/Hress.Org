import React, { useState } from 'react';
import './App.css';
import { BrowserRouter as Router, Route, Switch, Link } from 'react-router-dom';
import jwt from 'jsonwebtoken';
import ReactGA from 'react-ga4';

import PrivateRoute from './../components/access/privateRoute';
import { AuthContext } from './../context/auth';
import Hardhead from './hardhead';
import HardheadSidebar from './hardhead/sidebar';
import Awards from './hardhead/awards';
import Rules from './hardhead/rules';
import Statistics from './hardhead/statistics';
import Admin from './hardhead/admin';
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

function App(props) {
  const [authTokens, setAuthTokens] = useState();
  const [data, setData] = useState({ showMenu: false });
  const TRACKING_ID = "G-Z6HJ4VPZTN";


  const setTokens = (data) => {
    localStorage.setItem("tokens", JSON.stringify(data));
    var decodedToken = jwt.decode(data.token, { complete: true });
    localStorage.setItem("userID", decodedToken.payload.sub);
    setAuthTokens(data);
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
              <Route exact path="/" component={News} />
              <Route exact path="/hardhead" component={Hardhead} />
              <Route exact path="/hardhead/awards" component={Awards} />
              <Route path="/hardhead/awards/year/:id" component={AwardsByYear} />
              <Route exact path="/hardhead/awards/nominations" component={Nominations} />
              <Route exact path="/hardhead/awards/election" component={Election} />
              <Route path="/hardhead/awards/:id" component={AwardsByType} />
              <Route path="/hardhead/rules" component={Rules} />
              <Route path="/hardhead/users/:id" component={HHUsers} />
              <Route path="/hardhead/stats" component={Statistics} />
              <Route path="/hardhead/:hardheadID" component={HardheadEdit} />
              <PrivateRoute path="/hardhead/admin" component={Admin} />
              <Route exact path="/login" component={Login} />
              <Route path="/login/magic" component={Magic} />
              <Route path="/album" component={LegacyFrame} />
              <Route path="/chat" component={LegacyFrame} />
              <Route path="/comic" component={LegacyFrame} />
              {/* <Route path="/default/history" component={HistoryNews} /> */}
              <Route path="/default/single.aspx" component={LegacyRedirect} />              
              <Route path="/default" component={LegacyFrame} />
              <Route path="/feed" component={LegacyFrame} />
              <Route path="/foodandredwine" component={LegacyFrame} />
              <Route path="/dinnerparties/courses/:typeID" component={Election2022} />
              <Route path="/dinnerparties/:id" component={DinnerParty} />
              <Route path="/dinnerparties" component={DinnerParties} />
              <Route path="/gang" component={LegacyFrame} />
              <Route path="/hressgames" component={LegacyFrame} />
              <Route path="/mission" component={LegacyFrame} />
              <Route path="/news/history" component={HistoryNews} />
              <Route path="/news/:id" component={SingleNews} />
              <Route path="/rss" component={LegacyFrame} />
              <Route path="/yearly" component={LegacyFrame} />
              {/* <Route component={App} /> */}
            </Switch>

            {/* Sidebar */}
            <Switch>
              <Route exact path="/" component={MainSidebar} />
              <Route exact path="/hardhead" component={HardheadSidebar} />
              <Route exact path="/hardhead/awards" component={AwardsSidebar} />
              <Route path="/hardhead/users/:id" component={HHUserSidebar} />
              <Route path="/hardhead/awards/:id" /*component={AwardsSidebar}*/ />
              <Route path="/hardhead/:hardheadID" />
              <Route path="/hardhead/awards/year/:id" />
              <Route path="/hardhead/awards/election" />
              <Route path="/hardhead/awards/nominations" />
              <Route path="/hardhead/rules" />
              <Route path="/hardhead/stats" />
              <Route path="/news/history" component={HistorySidebar} />
            </Switch>
          </div>
          <Menu visible={data.showMenu} onClick={() => toggleMenu(true)} />
        </Router>
      </div>
    </AuthContext.Provider>
  );
}

export default App;
