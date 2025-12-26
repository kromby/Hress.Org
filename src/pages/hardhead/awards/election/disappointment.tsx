import axios from "axios";
import { useEffect, useState } from "react";
import config from "react-global-configuration";
import { Post } from "../../../../components";
import { useAuth } from "../../../../context/auth";
import { isMobile } from "react-device-detect";
import { Nomination } from "../../../../types/nomination";
import { ElectionModuleProps } from ".";
import { useLocation, useNavigate } from "react-router-dom";

const Disappointment = ({ ID, Name, onSubmit }: ElectionModuleProps) => {
  const { authTokens } = useAuth();
  const [disappointments, setDisappointments] = useState<Nomination[]>([]);
  const [selectedValue, setSelectedValue] = useState<string>();
  const [error, setError] = useState("");
  const [savingAllowed, setSavingAllowed] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    const getNominations = async () => {
      const url = `${config.get(
        "apiPath"
      )}/api/hardhead/awards/nominations?type=${ID}`;
      try {
        const response = await axios.get(url, {
          headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
        });
        setDisappointments(response.data);
      } catch (e) {
        console.error(e);
        setError(`Villa við að sækja lista af moðhaus tilnefningum: ${e}`);
      }
    };

    if (disappointments.length < 1) {
      getNominations();
    }
  }, [ID, authTokens]);

  const submitVote = async (voteValue: number) => {
    setSavingAllowed(false);
    if (authTokens === undefined) {
      navigate("/login", { state: { from: location.pathname } });
      return;
    }

    const voteData = [{ id: selectedValue, Value: voteValue }];

    try {
      const url = `${config.get("apiPath")}/api/elections/${ID}/vote`;
      await axios.post(url, voteData, {
        headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
      });
    } catch (e) {
      console.error(e);
      setError(`Villa við að kjósa moðhaus: ${e}`);
      setSavingAllowed(true);
    }

    onSubmit();
  };

  const handleSubmit = async () => {
    const voteValue = disappointments.filter((n) => n.id === selectedValue)[0]
      .nominee.id;
    await submitVote(voteValue);
  };

  const handleSkip = async () => {
    await submitVote(-1);
  };

  const handleChange = (event: string) => {
    setSelectedValue(event);
    setSavingAllowed(true);
  };

  return (
    <div>
      <Post
        id={ID}
        title={Name}
        body={
          <section>
            <div className="row gtr-uniform">
              {disappointments
                ? disappointments.map((nomination) => (
                    <div
                      className={isMobile ? "col-12" : "col-6"}
                      key={nomination.id}
                      onClick={() => handleChange(nomination.id)}
                    >
                      <input
                        type="radio"
                        checked={selectedValue === nomination.id}
                        onChange={() => handleChange(nomination.id)}
                      />
                      <label>
                        <h3
                          className="author"
                          style={{ width: "50%", paddingLeft: "125px" }}
                        >
                          {nomination.nominee.profilePhoto ? (
                            <img
                              src={`${config.get("apiPath")}${
                                nomination.nominee.profilePhoto.href
                              }?height=40&width=40`}
                              alt={nomination.nominee.name}
                            />
                          ) : null}
                          &nbsp;&nbsp;&nbsp;
                          <b>{nomination.nominee.name}</b>
                        </h3>
                      </label>
                      <br />
                      {nomination.description}
                      <br />
                      <br />
                      <u>Brot á eftirfarandi reglu:</u>
                      <br />
                      {nomination.affectedRule}
                      <br />
                      <br />
                    </div>
                  ))
                : null}
            </div>
          </section>
        }
      />
      <ul className="actions pagination">
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
      </ul>
    </div>
  );
};

export default Disappointment;
