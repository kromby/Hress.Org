import { Route, Navigate } from 'react-router-dom';
import { useAuth } from './../../context/auth';

function PrivateRoute({ component: Component, ...rest }) {
    const { authTokens } = useAuth();
  
    return (
      <Route        
        {...rest}
        render={props =>
          authTokens ? (
            <Component {...props} />
          ) : (
            <Navigate to={"/login?authTokens=" + authTokens} state={{ referer: props.location }} />
          )
        }
      />
    );
  }
  
  export default PrivateRoute;