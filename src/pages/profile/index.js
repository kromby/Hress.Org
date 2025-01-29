import axios from "axios";
import { useEffect, useState } from "react";
import config from "react-global-configuration";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { Post } from "../../components";
import Author from "../../components/author";
import { useAuth } from "../../context/auth";
import { useUsers } from "../../hooks/useUsers";

const Profile = () => {
  const { authTokens } = useAuth();
  const navigate = useNavigate();
  const [balanceSheet, setBalanceSheet] = useState();
  const [includePaid, setIncludePaid] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState(0);

  const location = useLocation();
  const userID = localStorage.getItem("userID");
  const { users, loading } = userID == 2630 ? useUsers("") : { users: null, loading: false };

  useEffect(() => {
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
      return;
    }

    const getBalanceSheet = async () => {
      const url = `${config.get("apiPath")}/api/users/${selectedUserId}/balancesheet${
        includePaid ? "?includePaid=true" : ""
      }`;
      try {
        const response = await axios.get(url, {
          headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
        });
        setBalanceSheet(response.data);
      } catch (e) {
        console.error(e);
      }
    };

    document.title = "Fjármál | Hress.Org";
    getBalanceSheet();
  }, [includePaid, selectedUserId]);

  return (
    <div id="main">
      {balanceSheet ? (
        <Post
          title="Hress sjóðurinn"
          description="Reikningsnr.: 0702-15-090979 - Kt: 090979-3029"
          body={
            <section>
              <h3 key="Header3">Útistandandi færslur</h3>
              <div className="table-wrapper" key="Table4">
                <table>
                  <thead>
                    <tr>
                      <th>Notandi</th>
                      <th>Dagsetning</th>
                      <th>Lýsing</th>
                      <th>Upphæð</th>
                    </tr>
                  </thead>
                  <tbody>
                    {balanceSheet.transactions.map((transaction) => (
                      <tr key={transaction.id} style={{ color: transaction.deleted ? 'lightgrey' : null }}>
                        <td>
                          {transaction.user.profilePhoto ? (
                            <Author
                              ID={transaction.user.id}
                              Username={transaction.user.name}
                              ProfilePhoto={transaction.user.profilePhoto.href}
                            />
                          ) : (
                            <Author
                              ID={transaction.user.id}
                              Username={transaction.user.name}
                            />
                          )}
                        </td>
                        <td>{transaction.insertedString}</td>
                        <td>
                          {transaction.deleted ? (
                            <s>{transaction.name}</s>
                          ) : (
                            transaction.name
                          )}
                        </td>
                        <td>
                          {transaction.deleted ? (
                            <s>{transaction.amount}</s>
                          ) : (
                            transaction.amount
                          )}
                          kr.
                        </td>
                      </tr>
                    ))}
                    <tr>
                      <td />
                      <td />
                      <td>
                        <b>Samtals</b>
                      </td>
                      <td>
                        <b>{balanceSheet.balance} kr.</b>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div style={{ marginBottom: "20px" }}>
                <input
                  type="checkbox"
                  id="cbx"
                  checked={includePaid}
                  onChange={() => setIncludePaid(!includePaid)}
                />
                <label htmlFor="cbx">Sýna greiddar færslur</label>
              </div>
              {authTokens && userID == 2630 && users && !loading && (
                <div style={{ marginBottom: "50px" }}>
                  <label htmlFor="userSelect" style={{ marginRight: "10px" }}>Sjá færslur fyrir annan Hressling</label>
                  <select 
                    id="userSelect"
                    value={selectedUserId}
                    onChange={(e) => setSelectedUserId(Number(e.target.value))}
                    style={{ padding: "5px", paddingLeft: "10px" }}
                  >
                    <option value={0}>--</option>
                    {users.map(user => (
                      <option key={user.ID} value={user.ID}>
                        {user.Name}
                      </option>
                    ))}
                  </select>
                </div>
              )}              
              <Link to={"/profile/password"}>
                <h3>Breyta lykilorði</h3>
              </Link>
              <Link
                to={"/Gang/Profile/MyProfile.aspx?legacy=true"}
                target="_blank"
              >
                <h3>Prófíllinn á gamla hress</h3>
              </Link>
            </section>
          }
        />
      ) : null}
    </div>
  );
};

export default Profile;
