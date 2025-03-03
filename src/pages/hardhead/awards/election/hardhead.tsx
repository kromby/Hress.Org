import { useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import Post from "../../../../components/post";
import { useAuth } from "../../../../context/auth";
import { isMobile } from "react-device-detect";
import { ElectionModuleProps } from ".";
import { useLocation, useNavigate } from "react-router-dom";
import { useHardheadUsersByHref } from "../../../../hooks/hardhead/useHardheadUsers";

const HardheadOfTheYear = ({
  ID,
  Name,
  Href,
  onSubmit,
}: ElectionModuleProps) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { authTokens } = useAuth();
  const [savingAllowed, setSavingAllowed] = useState(false);
  const [selectedUser, setSelectedUser] = useState<number | undefined>();

  const userID = Number(localStorage.getItem("userID"));

  const { users } = useHardheadUsersByHref(Href, userID);

  const handleUserChange = (event: number | undefined) => {
    setSelectedUser(event);
    setSavingAllowed(true);
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    setSavingAllowed(false);
    event.preventDefault();

    try {
      const url = `${config.get("apiPath")}/api/elections/${ID}/vote`;
      await axios.post(
        url,
        [
          {
            value: selectedUser,
          },
        ],
        {
          headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
        }
      );
    } catch (e) {
      console.error(e);
      alert(e);
      setSavingAllowed(true);
    }

    onSubmit();
  };

  if (authTokens === undefined) {
    navigate("/login", { state: { from: location.pathname } });
  }

  return (
    <Post
      id={ID}
      title={Name}
      body={
        <section>
          <form onSubmit={handleSubmit}>
            <div className="row gtr-uniform">
              {users
                ? users.map((user) => (
                    <div
                      className={isMobile ? "col-12" : "col-4"}
                      key={user.id}
                      onClick={() => handleUserChange(user.id)}
                    >
                      <input
                        type="radio"
                        checked={selectedUser === user.id}
                        onChange={() => handleUserChange(user.id)}
                      />
                      <label htmlFor={user.id.toString()}>
                        <h3
                          className="author"
                          style={{ width: "50%", marginLeft: "40px" }}
                        >
                          <img
                            src={`${config.get("apiPath")}${
                              user.profilePhoto?.href ??
                              "/api/images/278634/content"
                            }?width=50&height=50`}
                            alt={user.name}
                            style={{
                              marginRight: "10px",
                            }}
                          />
                          <b>{user.name}</b>
                        </h3>
                      </label>
                      <br />
                      Mætti á {user.attended} kvöld
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
                      value={`Kjósa ${Name}`}
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
