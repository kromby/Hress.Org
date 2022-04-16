import React, { useState } from 'react';
import './App.css';
import { BrowserRouter as Router, Route, Switch, Link } from 'react-router-dom';
import jwt from 'jsonwebtoken';

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
import AwardSingle from './hardhead/awards/awardByType';
import AwardsSidebar from './hardhead/awards/awardsSidebar';
import AwardsByYear from './hardhead/awards/awardsByYear';
import AwardsByType from './hardhead/awards/awardByType';
import HHUsers from './hardhead/hhusers';

function App(props) {
  const [authTokens, setAuthTokens] = useState();  
  const [data, setData] = useState({showMenu: false});

  const setTokens = (data) => {
    localStorage.setItem("tokens", JSON.stringify(data));
    var decodedToken = jwt.decode(data.token, {complete: true});
    localStorage.setItem("userID", decodedToken.payload.sub);
    setAuthTokens(data);
  }

  const checkExistingTokens = () => {
    if(authTokens === undefined) {
      var existingTokens = JSON.parse(localStorage.getItem("tokens"));
      if(existingTokens !== undefined && existingTokens !== null) {

        if(existingTokens.token === null) {
          console.log("checkExistingTokens - Faulty! remove from local storage");
          localStorage.removeItem("tokens");
          return;
        }

        var decodedToken = jwt.decode(existingTokens.token, {complete: true});
        var dateNow = new Date();
        if((decodedToken.payload.exp*1000) < (dateNow.getTime()+1)) {
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

  checkExistingTokens();

  const toggleMenu = () => {
    // console.log(showMenu);
    // setData({showMenu});
    var visible = !data.showMenu;
    
    if(visible)
      setData({showMenu: visible, class: "is-menu-visible"})
    else
      setData({showMenu: visible, class: ""})
  }

  return (
    <AuthContext.Provider value={{ authTokens, setAuthTokens: setTokens }}>
      <div className={data.class}>      
        <Router>
        <div id="wrapper" >
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
            <nav className="main">
              <ul>
                <li className="search">
                  <a className="fa-search" href="#search">Search</a>
                  <form id="search" method="get" action="#">
                    <input type="text" name="query" placeholder="Search" />
                  </form>
                </li>
                <li className="menu" onClick={() => toggleMenu(true)}>
                  {/* <a className="fa-bars" href="#menu">Menu</a> */}
                  <span className="fa-bars">Menu</span>
                </li>                
              </ul>
            </nav>
          </header>

          

          {/* <section id="menu">
              <section>
                <form className="search" method="get" action="#">
                  <input type="text" name="query" placeholder="Search" />
                </form>
              </section>

              <section>
                <ul className="links">
                  <li>
                    <a href="#">
                      <h3>Lorem ipsum</h3>
                      <p>Feugiat tempus veroeros dolor</p>
                    </a>
                  </li> 
                </ul>
              </section>

              <section>
                <ul className="actions stacked">
                  <li><a href="#" className="button large fit">Log In</a></li>
                </ul>
              </section>

            </section>           */}

        {/* Main section */}
        <Switch>          
          <Route exact path="/" component={Hardhead} />
          <Route exact path="/hardhead" component={Hardhead} />  
          <Route exact path="/hardhead/awards" component={Awards} /> 
          <Route path="/hardhead/awards/year/:id" component={AwardsByYear} />
          <Route path="/hardhead/awards/:id" component={AwardsByType} /> 
          <Route path="/hardhead/awards/election" component={Election} /> 
          <Route path="/hardhead/rules" component={Rules} />
          <Route path="/hardhead/users/:id" component={HHUsers} />
          <Route path="/hardhead/stats" component={Statistics} />
          <Route path="/hardhead/:hardheadID" component={HardheadEdit} />
          <PrivateRoute path="/hardhead/admin" component={Admin} />
          <Route exact path="/login" component={Login} />
          <Route path="/login/magic" component={Magic} />
          <Route component={App} />
        </Switch>
      
        {/* Sidebar */}
        <Switch>
          <Route exact path="/" component={HardheadSidebar}/>
          <Route exact path="/hardhead" component={HardheadSidebar}/> 
          <Route exact path="/hardhead/awards" component={AwardsSidebar} />
          <Route path="/hardhead/awards/:id" /*component={AwardsSidebar}*/ />
          <Route path="/hardhead/:hardheadID" />              
          <Route path="/hardhead/awards/year/:id" />          
          <Route path="/hardhead/awards/election" />
          <Route path="/hardhead/rules" />     
          <Route path="/hardhead/stats" />   
        </Switch>
        </div>
        <Menu visible={data.showMenu} onClick={() => toggleMenu(true)} />
      </Router>           
    </div>
  </AuthContext.Provider>
  );
}

export default App;
