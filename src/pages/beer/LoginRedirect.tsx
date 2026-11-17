import { Navigate, useLocation } from "react-router-dom";

// Sends the user to the login page, which sends them back here afterwards
// (src/pages/frame/login.js reads `location.state.from`), like the album pages.
const LoginRedirect = () => {
  const location = useLocation();

  return <Navigate to="/login" state={{ from: location.pathname }} replace />;
};

export default LoginRedirect;
