import * as Sentry from "@sentry/react";
import { useState } from "react";
import { BrowserRouter, Link, Routes, Route } from "react-router-dom";
import jwt from "jsonwebtoken";
import ReactGA from "react-ga4";

import { AuthContext } from "./../context/auth";
import Hardhead from "./hardhead";
import HardheadSidebar from "./hardhead/sidebar";
import Awards from "./hardhead/awards";
import Rules from "./hardhead/rules";
import Statistics from "./hardhead/statistics";
import Login from "./frame/login";
import Magic from "./frame/magic";
import Menu from "./frame/menu";
import HardheadEdit from "./hardhead/hardheadEdit";
import Election from "./hardhead/awards/election";
import AwardsSidebar from "./hardhead/awards/awardsSidebar";
import AwardsByYear from "./hardhead/awards/awardsByYear";
import AwardsByType from "./hardhead/awards/awardByType";
import HHUsers from "./hardhead/hhusers";
import HHUserSidebar from "./hardhead/hhusers/hhUserSidebar";
import Nominations from "./hardhead/awards/nominations";
import LegacyFrame from "./frame/legacyFrame";
// import LegacyRedirect from "./frame/legacyRedirect";
import Navigation from "./frame/navigation";
import News from "./news";
import MainSidebar from "./news/mainSidebar";
import SingleNews from "./news/singleNews";
import { Helmet } from "react-helmet";
import HistoryNews from "./news/history";
import HistorySidebar from "./news/historySidebar";
import DinnerParties from "./dinnerparties";
import DinnerParty from "./dinnerparties/dinnerparty";
import Profile from "./profile";
import Albums from "./albums";
import Album from "./albums/album";
import AlbumEdit from "./albums/albumEdit";
import RuleChange from "./hardhead/rules/change";
import Password from "./profile/password";
import DinnerPartySidebar from "./dinnerparties/sidebar";
import MovieList from "./hardhead/list";
import AlbumImageUpload from "./albums/albumImageUpload";

const SentryRoutes = Sentry.withSentryReactRouterV6Routing(Routes);

function App() {
  const [authTokens, setAuthTokens] = useState();
  const [data, setData] = useState({ showMenu: false });
  const TRACKING_ID = "G-Z6HJ4VPZTN";

  const setTokens = (tokenData) => {
    if (tokenData) {
      localStorage.setItem("tokens", JSON.stringify(tokenData));
      const decodedToken = jwt.decode(tokenData.token, { complete: true });
      localStorage.setItem("userID", decodedToken.payload.sub);
      Sentry.setUser({ id: decodedToken.payload.sub });
      setAuthTokens(tokenData);
    } else {
      setAuthTokens();
      localStorage.removeItem("tokens");
    }
  };

  const setGoogleAnalytics = () => {
    ReactGA.initialize(TRACKING_ID);
    ReactGA.send("pageview");
  };

  const checkExistingTokens = () => {
    if (authTokens === undefined) {
      const existingTokens = JSON.parse(localStorage.getItem("tokens"));
      if (existingTokens !== undefined && existingTokens !== null) {
        if (existingTokens.token === null) {
          localStorage.removeItem("tokens");
          return;
        }

        const decodedToken = jwt.decode(existingTokens.token, {
          complete: true,
        });
        const dateNow = new Date();
        if (decodedToken.payload.exp * 1000 < dateNow.getTime() + 1) {
          localStorage.removeItem("tokens");
        } else {
          localStorage.setItem("userID", decodedToken.payload.sub);
          setAuthTokens(existingTokens);
        }
      }
    }
  };

  setGoogleAnalytics();

  checkExistingTokens();

  const toggleMenu = () => {
    const visible = !data.showMenu;

    if (visible) setData({ showMenu: visible, class: "is-menu-visible" });
    else setData({ showMenu: visible, class: "" });
  };

  return (
    <AuthContext.Provider value={{ authTokens, setAuthTokens: setTokens }}>
      <Helmet>
        <meta property="og:title" key="og:title" content="Hress.Org" />
        <meta property="og:url" key="og:url" content={window.location.href} />
      </Helmet>
      <div className={data.class}>
        <BrowserRouter>
          <div id="wrapper">
            <header id="header">
              <h1>
                <Link to="/">Hress.Org</Link>
              </h1>
              <Navigation />
              <nav className="main">
                <ul>
                  <li className="search" onClick={() => toggleMenu()}>
                    <a className="fa-search" href="#search">
                      Search
                    </a>
                    {/* <form id="search" method="get" action="#">
                    <input type="text" name="query" placeholder="Search" />
                  </form> */}
                  </li>
                  <li className="menu" onClick={() => toggleMenu()}>
                    <a className="fa-bars" href="#menu">
                      Menu
                    </a>
                  </li>
                </ul>
              </nav>
            </header>

            {/* Main section */}
            <SentryRoutes>
              <Route exact path="/" element={<News />} />
              <Route path="album">
                <Route path="" element={<Albums />} />
                <Route path="edit" element={<AlbumEdit />} />
                <Route path=":id" element={<Album />}>
                  <Route path="edit" element={<AlbumEdit />} />
                </Route>
                <Route path=":id/upload" element={<AlbumImageUpload />} />
              </Route>
              <Route path="hardhead">
                <Route path="" element={<Hardhead />} />
                <Route path=":hardheadID" element={<Hardhead />} />
                <Route path=":hardheadID/edit" element={<HardheadEdit />} />
                <Route path="awards">
                  <Route path="" element={<Awards />} />
                  <Route path=":id" element={<AwardsByType />} />
                  <Route path="election" element={<Election />} />
                  <Route path="nominations" element={<Nominations />} />
                  <Route path="year">
                    <Route path=":id" element={<AwardsByYear />} />
                  </Route>
                </Route>
                <Route path="list" element={<MovieList />} />
                <Route path="rules">
                  <Route path="" element={<Rules />} />
                  <Route path="change" element={<RuleChange />} />
                </Route>
                <Route path="stats" element={<Statistics />} />
                <Route path="users">
                  <Route path=":id" element={<HHUsers />} />
                </Route>
                <Route path="films.aspx" element={<LegacyFrame />} />
                <Route path="defaultold.aspx" element={<LegacyFrame />} />
              </Route>
              <Route path="login">
                <Route exact path="" element={<Login />} />
                <Route path="magic" element={<Magic />} />
              </Route>
              <Route path="chat" element={<LegacyFrame />} />
              <Route path="comic" element={<LegacyFrame />} />
              <Route path="default">
                <Route path="*" element={<LegacyFrame />} />
              </Route>
              <Route path="feed" element={<LegacyFrame />} />
              <Route path="foodandredwine" element={<LegacyFrame />} />
              <Route path="dinnerparties">
                <Route path="" element={<DinnerParties />} />
                <Route path=":id" element={<DinnerParty />} />
                {/* <Route path="dinnerparties/courses/:typeID" element={<Election2022/>} /> */}
              </Route>
              <Route path="gang" element={<LegacyFrame />} />
              <Route path="hressgames" element={<LegacyFrame />} />
              <Route path="mission" element={<LegacyFrame />} />
              <Route path="news">
                <Route path=":id" element={<SingleNews />} />
                <Route path="history" element={<HistoryNews />} />
              </Route>
              <Route path="profile">
                <Route path="" element={<Profile />} />
                <Route path="password" element={<Password />} />
              </Route>
              <Route path="rss" element={<LegacyFrame />} />
              <Route path="yearly" element={<LegacyFrame />} />
            </SentryRoutes>

            {/* Sidebar */}
            <Routes>
              <Route exact path="/" element={<MainSidebar />} />
              <Route path="dinnerparties" element={<DinnerPartySidebar />} />
              <Route exact path="hardhead" element={<HardheadSidebar />} />
              <Route exact path="hardhead/awards" element={<AwardsSidebar />} />
              <Route path="hardhead/users/:id" element={<HHUserSidebar />} />
              <Route path="news/history" element={<HistorySidebar />} />
            </Routes>
          </div>
          <Menu visible={data.showMenu} onClick={() => toggleMenu()} />
        </BrowserRouter>
      </div>
    </AuthContext.Provider>
  );
}

export default App;
