import { useEffect, useState } from "react";
import { ErrorBoundary } from "react-error-boundary";
import axios from "axios";
import config from 'react-global-configuration';
import { Post } from "../../components";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../context/auth";
import { useLocation } from "react-router-dom";

const Albums = () => {
    const { authTokens } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();
    const [albums, setAlbums] = useState();
 
    useEffect(() => {        
        if (authTokens === undefined) {
            navigate("/login", {state: { from: location.pathname }} );
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
                onError={(error, errorInfo) => errorService.log({ error, errorInfo })}
                onReset={() => {
                    // reset the state of your app so the error doesn't happen again
                }}
            >

                <Post
                    title="Myndaalbúm"
                    body={
                        <div className="table-wrapper" key="Table4">
                            <table>
                                {/* <thead>
                                    <tr>
                                        <th>Harðhaus</th>
                                        <th>Útskýring</th>
                                    </tr>
                                </thead> */}
                                <tbody>
                                    {albums ? albums.map(album =>
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
                    }
                />
            </ErrorBoundary>
        </div>
    )
}

export default Albums;