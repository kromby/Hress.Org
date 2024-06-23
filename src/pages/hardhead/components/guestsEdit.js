import { useState, useEffect } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { useAuth } from "../../../context/auth";
import UserImage from "../../../components/users/userimage";
import { useLocation, useNavigate } from "react-router-dom";

const GuestsEdit = ({ hardheadID, users }) => {
  const { authTokens } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [guests, setGuests] = useState();

  const getGuests = () => {
    const url = `${config.get("apiPath")}/api/hardhead/${hardheadID}/guests`;
    axios
      .get(url)
      .then((response) => {
        setGuests(response.data);
      })
      .catch((error) => {
        if (error.response.status === 404) {
          console.log(
            "[GuestsEdit] Guests not found for Hardhead: " + hardheadID
          );
        } else {
          console.error(
            "[GuestsEdit] Error retrieving guests for Hardhead: " + hardheadID
          );
          console.error(error);
        }
      });
  };

  useEffect(() => {
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
      return;
    }

    if (!guests) {
      getGuests();
    }
  }, [hardheadID, authTokens]);

  const handleGuestChange = async (event) => {
    if (authTokens !== undefined) {
      event.preventDefault();
      try {
        const guestID = event.target.value;
        const url = `${config.get(
          "apiPath"
        )}/api/hardhead/${hardheadID}/guests/${guestID}`;
        await axios.post(
          url,
          {},
          {
            headers: { "X-Custom-Authorization": "token " + authTokens.token },
          }
        );
        getGuests();
      } catch (e) {
        console.error("[GuestsEdit] Ekki tókst að bæta gest við.");
        console.error(e);
      }
    }
  };

  return (
    <section>
      <h3>Gestir</h3>
      <div className="row gtr-uniform">
        <div className="col-12">
          {users ? (
            <select
              id="demo-category"
              name="demo-category"
              onChange={(ev) => handleGuestChange(ev)}
            >
              <option value="">- Veldu gest? -</option>
              {users
                .sort((a, b) => (a.Name > b.Name ? 1 : -1))
                .map((user) => (
                  <option key={user.ID} value={user.ID}>
                    {user.Name}
                  </option>
                ))}
            </select>
          ) : null}
        </div>
        {guests
          ? guests.map((guest) => (
              <div className="col-2 col-12-xsmall align-center" key={guest.id}>
                <UserImage
                  id={guest.id}
                  username={guest.username}
                  profilePhoto={guest.profilePhoto?.href}
                />
              </div>
            ))
          : "Enginn skráður gestur"}
      </div>
    </section>
  );
};

export default GuestsEdit;
