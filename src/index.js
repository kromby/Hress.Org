import React from 'react';
import ReactDOM from 'react-dom';
// import { BrowserRouter as Router, Route, Switch, Link } from 'react-router-dom';
import './index.css';
import * as serviceWorker from './serviceWorker';
import config from 'react-global-configuration';
import { PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider } from "@azure/msal-react";
import { msalConfig } from "./context/authConfig";

import App from './pages/App';
// import Hardhead from './pages/hardhead';
// import HardheadSidebar from './pages/hardhead/sidebar';
// import Awards from './pages/hardhead/awards';
// import Rules from './pages/hardhead/rules';
// import Statistics from './pages/hardhead/statistics';
// import Admin from './pages/hardhead/admin';

// const appInsights = require("applicationinsights");
// appInsights.setup("aea8e204-ca43-49b0-8b92-489055e8074d");
// appInsights.start();

// const routing = (    
//       <div id="wrapper">
//         <Router>
// 				<header id="header">
//           <h1><a href="http://www.hress.org" target="_parent">Hress.Org</a></h1>
//           <nav className="links">
//             <ul>
//               <li><Link to="/hardhead">Harðhaus</Link></li>
//               <li><a href="http://www.hress.org/yearly" target="_parent">Árlegt</a></li>
//               <li><a href="http://www.hress.org/links" target="_parent">Tenglar</a></li>
//               <li><a href="http://www.hress.org/comic" target="_parent">Comic</a></li>
//             </ul>
//           </nav>
//         </header>

//           {/* Main section */}
//           <Switch>          
//             <Route exact path="/" component={Hardhead} />
//             <Route exact path="/hardhead" component={Hardhead} />  
//             <Route path="/hardhead/awards" component={Awards} /> 
//             <Route path="/hardhead/rules" component={Rules} />
//             <Route path="/hardhead/stats" component={Statistics} />
//             <Route path="/hardhead/admin" component={Admin} />
//             <Route component={App} />
//           </Switch>
          
//           {/* Sidebar */}
//           <Switch>
//             <Route exact path="/" component={HardheadSidebar}/>
//             <Route exact path="/hardhead" component={HardheadSidebar}/>     
//             <Route path="/hardhead/awards" />                                       
//             <Route path="/hardhead/rules" />     
//             <Route path="/hardhead/stats" />   
//             <Route path="/hardhead/admin" />  
//             <Route component={App} />
//             </Switch>
//         </Router>       
//       </div>
//   )  

const msalInstance = new PublicClientApplication(msalConfig);

  function setConfig() {  
    config.set({ path: 'https://ezhressapi.azurewebsites.net', imagePath: 'https://ezcontentapi.azurewebsites.net', code: 'JRXXeaXTE5Y9WD2kVAYLu6gXknrmLlluqfTJZfo3pZfo4kkBUzf3Yw==', omdb: '8ae68ed6' }, {freeze: false, environment: 'prod'});
    config.set({ path: 'http://localhost:7071', imagePath: 'https://ezcontentapi.azurewebsites.net', code: '', omdb: '8ae68ed6'}, {freeze: false, environment: 'hybrid'});
    config.set({ path: 'http://localhost:7071', imagePath: 'https://localhost:44363', code: '', omdb: '8ae68ed6'}, {freeze: false, environment: 'dev'});
    config.set({ });
  
    config.setEnvironment('prod');
  }
    
  setConfig(); 

ReactDOM.render(<MsalProvider instance={msalInstance}><App/></MsalProvider>, document.getElementById('content'));

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
