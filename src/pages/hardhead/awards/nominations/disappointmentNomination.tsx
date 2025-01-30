import { useEffect, useState } from "react";
import { useAuth } from "../../../../context/auth";
import config from "react-global-configuration";
import axios from "axios";
import { Post } from "../../../../components";
import Author from "../../../../components/authorOld";

const DisappointmentNomination = ({ Type, Users }: { Type: string, Users: [] }) => {
  const { authTokens } = useAuth();
  const [buttonEnabled, setButtonEnabled] = useState(false);
  const [users, setUsers] = useState<any>();
  const [nominations, setNominations] = useState<any>();
  const [description, setDescription] = useState<string>();
  const [nominee, setNominee] = useState<string>();
  const [isSaved, setIsSaved] = useState(false);
  const [error, setError] = useState<string>();

  const getNominations = () => {
    const getUrl = `${config.get('apiPath')}/api/hardhead/awards/nominations?type=${Type}`;
    axios
      .get(getUrl, {
        headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
      })
      .then((response) => setNominations(response.data))
      .catch((axiosError) => {
        if (axiosError.response.status === 404) {
          console.log("[DisappointmentNomination] No nominations not found");
        } else {
          console.error("[DisappointmentNomination] Error getting access");
          console.error(axiosError);
        }
      });
  };

  useEffect(() => {
    setUsers(Users);

    document.title = "Tilnefna Vinnbrigði | Hress.Org";

    if (!nominations) {
      getNominations();
    }
  }, [Users]);

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    setButtonEnabled(false);
    event.preventDefault();

    try {
      const postUrl = `${config.get('apiPath')}/api/hardhead/awards/nominations`;
      await axios.post(
        postUrl,
        {
          typeID: Type,
          description,
          nomineeID: nominee,
        },
        {
          headers: { "X-Custom-Authorization": `token ${authTokens.token}` },
        }
      );
      setIsSaved(true);
      setDescription("");
      setNominee("");
      getNominations();
    } catch (e: any) {
      console.error(e);
      if (e?.response?.status === 400) {
        setError(`Ekki tókst að skrá tilnefningu! - ${e.message}`);
      } else {
        setError("Ekki tókst að skrá tilnefningu!");
      }
    }
  };

  const allowSaving = (nomineeID: string, descriptionText: string) => {
    if (descriptionText === undefined) return false;
    if (descriptionText.length <= 10 || nomineeID.length <= 0) {
      return false;
    }

    setIsSaved(false);
    setError("");
    return true;
  };

  const handleNomineeChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    setNominee(event.target.value);
    setButtonEnabled(allowSaving(event.target.value, description ?? ""));
  };
  const handleDescriptionChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    setDescription(event.target.value);
    setButtonEnabled(allowSaving(nominee ?? "", event.target.value));
  };

  return (
    <Post
      title="Vinbrigði ársins"
      description="Tilnefndu Harðhaus fyrir hörmuleg mistök"
      body={[
        <form onSubmit={handleSubmit} key="Form1">
          <div className="row gtr-uniform">
            <div className="col-6 col-12-xsmall">
              {users ? (
                <select
                  id="category"
                  name="category"
                  onChange={(ev) => handleNomineeChange(ev)}
                >
                  <option value="">- Hvaða Harðhaus vilt þú tilnefna? -</option>
                  {users
                    .sort((a: { Name: string }, b: { Name:string }) =>
                      a.Name.toLowerCase() > b.Name.toLowerCase() ? 1 : -1
                    )
                    .map((user: { ID: number, Name: string }) => (
                      <option key={user.ID} value={user.ID}>
                        {user.Name}
                      </option>
                    ))}
                </select>
              ) : null}
            </div>
            <div className="col-12">
              <textarea
                name="Lýsing"
                rows={3}
                onChange={(ev) => handleDescriptionChange(ev)}
                defaultValue={description}
                placeholder="Fyrir hvað vilt þú tilnefna?"
              />
            </div>
            <div className="col-12">
              {isSaved ? (
                <b>
                  Tilnefning skráð!
                  <br />
                </b>
              ) : null}
              {error ? (
                <b>
                  {error}
                  <br />
                </b>
              ) : null}
              <button
                title="Tilnefna"
                className="button large"
                disabled={!buttonEnabled}
              >
                Tilnefna
              </button>
            </div>
          </div>
        </form>,
        <hr key="Line2" />,
        <h3 key="Header3">Skráðar tilnefningar</h3>,
        <div className="table-wrapper" key="Table4">
          <table>
            <thead>
              <tr>
                <th style={{ width: '200px' }}>Harðhaus</th>
                <th style={{ width: '900px' }}>Útskýring</th>
                <th>Tilnefnandi</th>
              </tr>
            </thead>
            {nominations ? (
              <tbody>
                {nominations.map((nomination: any) => (
                  <tr key={nomination.id}>
                    <td>
                      {nomination.nominee.profilePhoto ? (
                        <Author
                          ID={nomination.nominee.id}
                          Username={nomination.nominee.name}
                          ProfilePhoto={nomination.nominee.profilePhoto.href}
                        />
                      ) : (
                        <Author
                          ID={nomination.nominee.id}
                          Username={nomination.nominee.name}
                        />
                      )}
                    </td>
                    <td>{nomination.description}</td>
                    <td>{nomination.insertedBy}</td>
                  </tr>
                ))}
              </tbody>
            ) : null}
          </table>
        </div>,
      ]}
    />
  );
};

export default DisappointmentNomination;
