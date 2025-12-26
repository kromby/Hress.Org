import { useState, useEffect } from "react";
import config from "react-global-configuration";
import axios from "axios";
import Post from "../../../../components/post";
import { useAuth } from "../../../../context/auth";
import { isMobile } from "react-device-detect";
import { ElectionModuleProps } from ".";
import { useLocation, useNavigate } from "react-router-dom";
import { Nomination } from "../../../../types/nomination";

const Stallone = ({
  ID,
  Name,
  Description,
  Date,
  Year,
  onSubmit,
}: ElectionModuleProps) => {
  const { authTokens } = useAuth();
  const [stallones, setStallones] = useState<Nomination[]>([]);
  const [savingAllowed, setSavingAllowed] = useState(false);
  const [selectedUser, setSelectedUser] = useState<string>();
  const [error, setError] = useState("");
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    const getNominations = async () => {
      try {
        const url = `${config.get(
          "apiPath"
        )}/api/hardhead/awards/nominations?type=${ID}`;
        const response = await axios.get(url, {
          headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
        });
        setStallones(response.data);
      } catch (e) {
        console.error(e);
        setError(`Villa við að sækja lista af Stallone tilnefningum: ${e}`);
      }
    };

    if (stallones.length < 1) {
      getNominations();
    }
  }, [ID]);

  const handleChange = (event: string) => {
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
      return;
    }

    const userID = localStorage.getItem("userID");
    if (event === userID) {
      setError(
        "Ætlar þú í alvöru að kjósa sjálfan þig, það er ekki mjög Harðhausalegt."
      );
      return;
    }

    setSelectedUser(event);
    setSavingAllowed(true);
  };

  const submitVote = async (voteValue: number) => {
    setSavingAllowed(false);
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
      return;
    }

    const voteData = [{ id: selectedUser, Value: voteValue }];

    try {
      const url = `${config.get("apiPath")}/api/elections/${ID}/vote`;
      await axios.post(url, voteData, {
        headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
      });
    } catch (e) {
      console.error(e);
      setError(`Villa við að kjósa breytingu: ${e}`);
      setSavingAllowed(true);
    }

    onSubmit();
  };

  const handleSubmit = async () => {
    const voteValue = stallones.filter((n) => n.id === selectedUser)[0].nominee
      .id;
    await submitVote(voteValue);
  };

  const handleSkip = async () => {
    await submitVote(-1);
  };

  return [
    <Post
      key="1"
      id={ID}
      title={Name}
      description={Description}
      date={Date}
      dateFormatted={Year}
      body={
        <section>
          <div className="row gtr-uniform">
            {stallones
              ? stallones.map((stallone) => (
                  <div
                    className={isMobile ? "col-12" : "col-6"}
                    key={stallone.id}
                    onClick={() => handleChange(stallone.id)}
                  >
                    <input
                      type="radio"
                      checked={selectedUser === stallone.id}
                      onChange={() => handleChange(stallone.id)}
                    />
                    <label>
                      <h3
                        className="author"
                        style={{ width: "50%", paddingLeft: "125px" }}
                      >
                        {stallone.nominee.profilePhoto ? (
                          <img
                            src={
                              config.get("apiPath") +
                              stallone.nominee.profilePhoto.href
                            }
                            alt={stallone.nominee.name}
                          />
                        ) : null}
                        &nbsp;&nbsp;&nbsp;
                        <b>{stallone.nominee.name}</b>
                      </h3>
                    </label>
                    <br />
                    {stallone.description}
                    <br />
                    <br />
                  </div>
                ))
              : null}
          </div>
        </section>
      }
    />,
    <ul key="2" className="actions pagination">
      {error ? (
        <li>
          <b>
            {error}
            <br />
          </b>
        </li>
      ) : null}
      <li>
        <button
          onClick={handleSubmit}
          disabled={!savingAllowed}
          className="button large next"
        >
          {`Kjósa ${Name}`}
        </button>
      </li>
      <li>
        <button onClick={handleSkip} className="button large">
          Sitja hjá
        </button>
      </li>
    </ul>,
  ];
};

export default Stallone;
