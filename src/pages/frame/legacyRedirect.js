import { useEffect } from "react";
import queryString from 'query-string';
import { useLocation, useNavigate } from "react-router-dom";

const LegacyRedirect = () => {
    const location = useLocation();
    const navigate = useNavigate();

    useEffect(() => {
        const parsed = queryString.parse(location.search);

        let path;

        if (location.pathname.toLowerCase() === '/default/single.aspx') {
            if (parsed.Id) {
                path = "/news/" + parsed.Id;
            } else if (parsed.id) {
                path = "/news/" + parsed.id;
            } else if (parsed.ID) {
                path = "/news/" + parsed.ID;
            } else {
                console.error("[LegacyRedirect] No ID found in query string");
            }
        } else {
            console.error("[LegacyRedirect] Unknown path: '" + location.pathname + "'");
        }

        

        if (path) {
            navigate(path, { state: { from: location.pathname } });
        }
    }, [])

    return (
        <div id="main">
            {location.pathname}
        </div>
    )
}

export default LegacyRedirect;