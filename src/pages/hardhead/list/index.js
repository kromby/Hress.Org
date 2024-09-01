import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { Post } from "../../../components";

const MovieList = () => {
  const [hardheads, setHardheads] = useState();

  useEffect(() => {
    const getHardheads = async () => {
      const url = `${config.get("apiPath")}/api/hardhead?dateFrom=1.1.1999`;
      try {
        const response = await axios.get(url);
        setHardheads(response.data);
      } catch (e) {
        console.error(e);
      }
    };

    document.title = "Harðhausamyndirnar | Hress.Org";

    // if (!hardheads) {
    getHardheads();
    // }
  }, []);

  return (
    <div id="main">
      <Post
        title="Myndirnar"
        description="Listi yfir allar myndirnar sem hafa verið sýndar"
        body={
          <div>
            <table>
              <thead>
                <tr>
                  <th />
                  <th>Kvöld</th>
                  <th>Mynd</th>
                  <th>Harðhaus</th>
                </tr>
              </thead>
              <tbody>
                {hardheads
                  ? hardheads.map((hardhead) => (
                      <tr key={hardhead.id}>
                        <td>
                          {hardhead.movie.posterPhoto?.href ? (
                            <img
                              src={`${config.get("apiPath")}${
                                hardhead.movie.posterPhoto?.href
                              }?width=80`}
                              alt={hardhead.name}
                              // style={{ height: imageHeight }}
                            />
                          ) : null}
                        </td>
                        <td style={{ verticalAlign: "top" }}>
                          <a href={`/hardhead/${hardhead.id}`}>
                            {hardhead.name}
                          </a>
                          <br />
                          {hardhead.dateString}
                          <br />
                          {hardhead.guestCount} gestir
                        </td>
                        <td style={{ verticalAlign: "top" }}>
                          {hardhead.movie.name}
                          <br />
                          (Dráp: {hardhead.movie.movieKillCount ?? "-"})
                        </td>
                        <td style={{ verticalAlign: "top" }}>
                          {hardhead.movie.actor}
                          <br />
                          (Dráp: {hardhead.movie.hardheadKillCount ?? "-"})
                        </td>
                      </tr>
                    ))
                  : null}
              </tbody>
            </table>
          </div>
        }
        actions={null}
        stats={null}
      />
    </div>
  );
};

export default MovieList;
