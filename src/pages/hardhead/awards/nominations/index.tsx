import { useEffect } from "react";
import { useHardheadUsers } from "../../../../hooks/hardhead/useHardheadUsers";
import StalloneNomination from "./stalloneNomination";
import DisappointmentNomination from "./disappointmentNomination";
import { useAuth } from "../../../../context/auth";
import { useLocation, useNavigate } from "react-router-dom";

const Nominations = () => {
  const { authTokens } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const userID = localStorage.getItem("userID");

  useEffect(() => {
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
    }
  }, [authTokens, navigate, location.pathname]);

  const { users, loading, error } = useHardheadUsers(
    Number(process.env.REACT_APP_NOMINATIONS_YEAR_ID) || 5437,
    Number(userID) || undefined
  );

  useEffect(() => {
    document.title = "Tilnefningar | Hress.Org";
  }, []);

  if (!authTokens) {
    return null;
  }

  if (loading) {
    return <div id="main">Loading...</div>;
  }

  if (error) {
    return <div id="main">Error: {error}</div>;
  }

  return (
    <div id="main">
      <StalloneNomination Users={users} Type="5284" />
      <DisappointmentNomination Users={users} Type="360" />
    </div>
  );
};

export default Nominations;
