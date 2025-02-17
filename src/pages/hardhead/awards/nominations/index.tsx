import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { useAuth } from "../../../../context/auth";
import StalloneNomination from "./stalloneNomination";
import DisappointmentNomination from "./disappointmentNomination";
import { useLocation, useNavigate } from "react-router-dom";

const Nominations = () => {
  const { authTokens } = useAuth();
  const location = useLocation();
  const [users, setUsers] = useState<any>();
  const navigate = useNavigate();

  const YEAR_ID = process.env.REACT_APP_NOMINATIONS_YEAR_ID || "5437";
  const url = `${config.get(
    "path"
  )}/api/hardhead/${YEAR_ID}/users?code=${config.get("code")}`;

  useEffect(() => {
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
      return;
    }

    const getUsers = async () => {
      try {
        const userID = localStorage.getItem("userID");
        const response = await axios.get(url);
        setUsers(
          response.data.filter((user: { ID: string }) => user.ID !== userID)
        );
      } catch (e) {
        console.error("Failed to fetch users:", e);
        setUsers([]);
      }
    };

    document.title = "Tilnefningar | Hress.Org";

    if (!users) {
      getUsers();
    }
  }, [url]);

  return (
    <div id="main">
      <StalloneNomination Users={users} Type="5284" />
      <DisappointmentNomination Users={users} Type="360" />
    </div>
  );
};

export default Nominations;
