
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
                        <a href={"https://hress.azurewebsites.net/magic/?code=d6148a6b2f4f456a898de6380a1fa814&path=" + link.link.href} target="_blank">{link.name}</a> :                        
                        <Link to={link.link.href}>{link.name}</Link>
                    }
                    </li>    
                ): null}
              </ul>
            </nav>
    )
}

export default Navigation;