import { useState, useEffect, SetStateAction } from "react";
import config from "react-global-configuration";
import axios from "axios";
import Post from "../../../../components/post";
import { useAuth } from "../../../../context/auth";
import { isMobile } from "react-device-detect";
import { ElectionModuleProps } from ".";
import { useLocation, useNavigate } from "react-router-dom";

const HardheadOfTheYear = ({
  ID,
  Name,
  Href,
  onSubmit,
}: ElectionModuleProps) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { authTokens } = useAuth();
  const [users, setUsers] = useState<any[]>();
  const [savingAllowed, setSavingAllowed] = useState(false);
  const [selectedUser, setSelectedUser] = useState<number | undefined>();

  useEffect(() => {
    const getHardheadUsers = async () => {
      try {
        const url = config.get("path") + Href + "&code=" + config.get("code");
        const response = await axios.get(url);
        setUsers(response.data);
      } catch (e) {
        console.error(e);
        alert(e);
      }
    };

    if (!users) {
      getHardheadUsers();
    }
  }, []);

  if (authTokens === undefined) {
    navigate("/login", { state: { from: location.pathname } });
    return;
  }

  const handleUserChange = (event: number | undefined) => {
    const userID = Number(localStorage.getItem("userID"));
    if (event === userID) {
      alert(
        "Ætlar þú í alvöru að kjósa sjálfan þig, það er ekki mjög Harðhausalegt."
      );
      return;
    }

    setSelectedUser(event);
    setSavingAllowed(true);
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    setSavingAllowed(false);
    event.preventDefault();

    try {
      const url = config.get("apiPath") + "/api/elections/" + ID + "/vote";
      await axios.post(
        url,
        [
          {
            value: selectedUser,
          },
        ],
        {
          headers: { "X-Custom-Authorization": "token " + authTokens.token },
        }
      );
    } catch (e) {
      console.error(e);
      alert(e);
      setSavingAllowed(true);
    }

    onSubmit();
  };

  return (
    <Post
      id={ID}
      title={Name}
      body={
        <section>
          <form onSubmit={handleSubmit}>
            <div className="row gtr-uniform">
              {users
                ? users.map((user: any) => (
                    <div
                      className={isMobile ? "col-12" : "col-4"}
                      key={user.ID}
                      onClick={() => handleUserChange(user.ID)}
                    >
                      <input
                        type="radio"
                        checked={selectedUser === user.ID}
                        onChange={() => handleUserChange(user.ID)}
                      />
                      <label>
                        <h3
                          className="author"
                          style={{ width: "50%", marginLeft: "40px" }}
                        >
                          {user.ProfilePhoto ? (
                            <img
                              src={`${config.get("apiPath")}${
                                user.ProfilePhoto.Href
                              }?width=50&height=50`}
                              alt={user.Name}
                              style={{
                                marginRight: "10px",
                              }}
                            />
                          ) : null}
                          <b>{user.Name}</b>
                        </h3>
                      </label>
                      <br />
                      Mætti á {user.Attended} kvöld
                      <br />
                      <br />
                      <br />
                    </div>
                  ))
                : "null"}
              <div className="col-12">
                <ul className="actions">
                  <li>
                    <input
                      type="submit"
                      value={"Kjósa " + Name}
                      disabled={!savingAllowed}
                    />
                  </li>
                </ul>
              </div>
            </div>
          </form>
        </section>
      }
    />
  );
};

export default HardheadOfTheYear;
