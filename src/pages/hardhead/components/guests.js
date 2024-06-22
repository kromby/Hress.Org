import { useEffect, useState } from "react";
import config from "react-global-configuration";
import UserImage from "../../../components/users/userimage";
import axios from "axios";

const Guests = ({ hardheadID }) => {
  const [error, setError] = useState();
  const [guests, setGuests] = useState();

  useEffect(() => {
    const getGuests = () => {
      const url = `${config.get("apiPath")}/api/hardhead/${hardheadID}/guests`;

      axios
        .get(url)
        .then((response) => setGuests(response.data))
        .catch((ex) => {
          console.error("[Guests] Error: ", ex);
          if (ex.response.status !== 404) {
            setError(ex);
            console.log("[Guests] Error retrieving guests");
          }
        });
    };

    if (!guests) {
      getGuests();
    }
  }, [hardheadID]);

  return (
    <section>
      <h3>Gestir</h3>
      <div className="row gtr-uniform">
        {guests
          ? guests.map((guest) => (
              <div className="col-2 align-center" key={guest.id}>
                <UserImage
                  id={guest.id}
                  username={guest.username}
                  profilePhoto={guest.profilePhoto?.href}
                />
              </div>
            ))
          : null}
        {error ? error : null}
      </div>
    </section>
  );
};

export default Guests;
