
import axios from "axios";
import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import { Link } from "react-router-dom";
import { useAuth } from "../../context/auth";

const Navigation = () => {
    const { authTokens } = useAuth();
    const [links, setLinks] = useState();
    const [loggedIn, setLoggedIn] = useState(false);

    var url = config.get("apiPath") + "/api/menus";

    useEffect(() => {
        const getLinks = async () => {
            try {
                var headers = authTokens ? { headers: { 'X-Custom-Authorization': 'token ' + authTokens.token } } : null;
                const response = await axios.get(url, headers);
                setLinks(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        if (!links || loggedIn != (authTokens != undefined)) {
            setLoggedIn(authTokens != undefined);
            getLinks();
        }
    }, [url])

    return (
        <nav className="links">
            <ul>
                {links ? links.map(link =>
                    <li key={link.id}>
                        {link.isLegacy ?
                            <Link to={link.link.href + "?legacy=true"} target="_blank">{link.name}</Link> :
                            <Link to={link.link.href}>{link.name}</Link>
                        }
                    </li>
                ) : null}
            </ul>
        </nav>
    )
}

export default Navigation;