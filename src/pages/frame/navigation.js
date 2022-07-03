
import axios from "axios";
import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import { Link } from "react-router-dom";
import { useAuth } from "../../context/auth";

const Navigation = (propsData) => {
    const {authTokens} = useAuth();
    const [links, setLinks] = useState();

    var url = config.get("apiPath") + "/api/menus";

    useEffect(() => {
        const getLinks = async () => {
            try {
                var token = authTokens ? authTokens.token : null;
                const response = await axios.get(url, {
                    headers: { 'X-Custom-Authorization': 'token ' + token }
                });
                setLinks(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        getLinks();
    }, [propsData, url])

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
                ): null}
              </ul>
            </nav>
    )
}

export default Navigation;