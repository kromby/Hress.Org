import React from 'react';
import { Outlet, Navigate } from 'react-router-dom';
import { useAuth } from './../../context/auth';

const PrivateRoute = () => {
  const { authTokens } = useAuth();

  // If authorized, return an outlet that will render child elements
  // If not, return element that will navigate to login page
  return auth ? <Outlet /> : <Navigate to={{ pathname: "/login?authTokens=" + authTokens, state: { referer: props.location } }} />;
}

// function PrivateRoute({ component: Component, ...rest }) {
//     const { authTokens } = useAuth();
  
//     return (
//       <Route        
//         {...rest}
//         render={props =>
//           authTokens ? (
//             <Component {...props} />
//           ) : (
//             <Navigate  to={{ pathname: "/login?authTokens=" + authTokens, state: { referer: props.location } }} />
//           )
//         }
//       />
//     );
//   }
  
  export default PrivateRoute;