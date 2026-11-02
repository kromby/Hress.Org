import { Link, useLocation } from "react-router-dom";

// Login link that sends the user back to the current page after logging in
// (src/pages/frame/login.js reads `location.state.from`).
const LoginLink = () => {
  const location = useLocation();

  return (
    <Link to="/login" state={{ from: location.pathname }}>
      Skrá inn
    </Link>
  );
};

export default LoginLink;
