import { useEffect, useState } from "react";
import { ErrorBoundary } from "react-error-boundary";
import { Link, useNavigate, useLocation } from "react-router-dom";
import axios from "axios";
import config from 'react-global-configuration';
import { isMobile } from "react-device-detect";
import { Post } from "../../components";
import { useAuth } from "../../context/auth";
import Preview from "./preview";

const Albums = () => {
    const { authTokens } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();
    const [albums, setAlbums] = useState();

    useEffect(() => {
        if (authTokens === undefined) {
            navigate("/login", { state: { from: location.pathname } });
            return;
        }

        const getAlbums = async () => {
            var url = config.get("apiPath") + "/api/albums";
            try {
                const response = await axios.get(url, {
                    headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
                });
                setAlbums(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        document.title = "Myndaalbúm | Hress.Org"

        if (!albums) {
            getAlbums();
        }
    }, [])

    return (
        <div id="main">
            <ErrorBoundary
                FallbackComponent={<div>Það vantar einhvern hressleika hér!</div>}
                onError={(error, errorInfo) => console.log({ error, errorInfo })}
                onReset={() => {
                    // reset the state of your app so the error doesn't happen again
                }}
            >

                <Post
                    title="Myndaalbúm"
                    body={[
                        <section key="One">
                            {albums ? albums.slice(0, 3).map(album =>
                                <div className="row gtr-uniform" key={album.id}>
                                    <div className={isMobile ? "col-12" : "col-4"}>
                                        <h4>
                                            <Link to={"/album/" + album.id}>{album.name}</Link>
                                        </h4>
                                        {album.insertedString}<br />
                                        {album.description}<br />
                                        {album.imageCount} mynd(ir)
                                    </div>
                                    <div className={isMobile ? "col-12" : "col-8"}>
                                        <Preview url={album.images.href} />
                                    </div>
                                    <p />
                                </div>
                            ) : null}
                        </section>,
                        <section key="divider">
                            <p />
                        </section>,
                        <section key="Two">
                            <div className="table-wrapper" key="Table4">
                                <table>
                                    {/* <thead>
                                    <tr>
                                        <th>Harðhaus</th>
                                        <th>Útskýring</th>
                                    </tr>
                                </thead> */}
                                    <tbody>
                                        {albums ? albums.slice(3).map(album =>
                                            <tr key={album.id}>
                                                <td><Link to={"/album/" + album.id}>{album.name}</Link></td>
                                                <td>{album.insertedString}</td>
                                                <td>{album.description}</td>
                                                <td>{album.imageCount} mynd(ir)</td>
                                            </tr>
                                        ) : null}
                                    </tbody>
                                </table>
                            </div>
                        </section>
                    ]}
                />
            </ErrorBoundary>
        </div >
    )
}

export default Albums;