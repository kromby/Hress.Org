import React, { useEffect } from "react";
import { useAuth } from "../../../context/auth";
import UserImage from "../../../components/users/userimage";
import { useLocation, useNavigate } from "react-router-dom";
import { useHardheadGuests } from "../../../hooks/hardhead/useHardheadGuests";
import "./guestsEdit.css";

export interface GuestCandidate {
  id: number;
  name?: string;
  username?: string;
  profilePhoto?: { href?: string };
}

interface GuestsEditProps {
  hardheadID: number | string;
  users?: GuestCandidate[];
  hostId?: number;
}

const GuestsEdit: React.FC<GuestsEditProps> = ({ hardheadID, users, hostId }) => {
  const { authTokens } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  const { guests, addGuest, removeGuest } = useHardheadGuests(
    hardheadID,
    authTokens?.token
  );

  useEffect(() => {
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
    }
  }, [authTokens, location.pathname, navigate]);

  const handleGuestChange = async (event: React.ChangeEvent<HTMLSelectElement>) => {
    if (authTokens !== undefined && event.target.value) {
      event.preventDefault();
      try {
        const guestID = Number(event.target.value);
        await addGuest(guestID);
      } catch (e) {
        console.error("[GuestsEdit] Ekki tókst að bæta gest við.");
        console.error(e);
      }
    }
  };

  const handleRemoveGuest = async (guestID: number) => {
    if (authTokens !== undefined) {
      try {
        await removeGuest(Number(guestID));
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
              >
                <UserImage
                  id={guest.id}
                  username={guest.username}
                  profilePhoto={guest.profilePhoto?.href}
                  text=""
                />
                <button
                  type="button"
                  className="remove-guest-btn"
                  onClick={() => handleRemoveGuest(guest.id)}
                  title="Fjarlægja gest"
                >
                  Fjarlægja
                </button>
              </div>
            ))
          : "Enginn skráður gestur"}
      </div>
    </section>
  );
};

export default GuestsEdit;
