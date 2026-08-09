import { useState, useEffect } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { useAuth } from "../../../context/auth";
import UserImage from "../../../components/users/userimage";
import { useLocation, useNavigate } from "react-router-dom";

const GuestsEdit = ({ hardheadID, users, hostId }) => {
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
        if (error.response?.status === 404) {
          console.log(
            "[GuestsEdit] Guests not found for Hardhead: ",
            hardheadID
          );
        } else {
          console.error(
            "[GuestsEdit] Error retrieving guests for Hardhead: ",
            hardheadID
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
    if (authTokens !== undefined && event.target.value) {
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
            headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
          }
        );
        getGuests();
      } catch (e) {
        console.error("[GuestsEdit] Ekki tókst að bæta gest við.");
        console.error(e);
      }
    }
  };

  const handleRemoveGuest = async (guestID) => {
    if (authTokens !== undefined) {
      try {
        const url = `${config.get(
          "apiPath"
        )}/api/hardhead/${hardheadID}/guests/${guestID}`;
        await axios.delete(url, {
          headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
        });
        getGuests();
      } catch (e) {
        console.error("[GuestsEdit] Ekki tókst að fjarlægja gest.");
        console.error(e);
      }
    }
  };

  const availableUsers = users
    ? users.filter(
        (user) =>
          user.id !== hostId &&
          !guests?.some((guest) => guest.id === user.id)
      )
    : [];

  return (
    <section>
      <h3>Gestir</h3>
      <div className="row gtr-uniform">
        <div className="col-12">
          {users ? (
            <select
              id="demo-category"
              name="demo-category"
              onChange={handleGuestChange}
              value=""
            >
              <option value="">- Veldu gest? -</option>
              {availableUsers.map((user) => (
                <option key={user.id} value={user.id}>
                  {user.name}
                </option>
              ))}
            </select>
          ) : null}
        </div>
        {guests && guests.length > 0
          ? guests.map((guest) => (
              <div
                className="col-2 col-12-xsmall align-center"
                key={guest.id}
                style={{ position: "relative" }}
              >
                <button
                  type="button"
                  onClick={() => handleRemoveGuest(guest.id)}
                  title="Fjarlægja gest"
                  style={{
                    position: "absolute",
                    top: "-8px",
                    right: "4px",
                    background: "#e74c3c",
                    color: "#fff",
                    border: "none",
                    borderRadius: "50%",
                    width: "22px",
                    height: "22px",
                    lineHeight: "22px",
                    padding: 0,
                    cursor: "pointer",
                    fontSize: "12px",
                    fontWeight: "bold",
                    zIndex: 2,
                  }}
                >
                  &times;
                </button>
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
