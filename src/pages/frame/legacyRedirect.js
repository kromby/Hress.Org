import { faWindows } from "@fortawesome/free-brands-svg-icons";
import { useEffect, useState } from "react";
import queryString from 'query-string';
import { Redirect } from "react-router-dom";
import { useLocation } from "react-router-dom-v5-compat";

const LegacyRedirect = () => {
    const [path, setPath] = useState();
    const location = useLocation();

    useEffect(() => {
        const parsed = queryString.parse(location.search);

        if (location.pathname.toLowerCase() === '/default/single.aspx') {
            if(parsed.Id) {
            setPath("/news/" + parsed.Id);
            } else if (parsed.id) {
                setPath("/news/" + parsed.id);
            } else if (parsed.ID) {
                setPath("/news/" + parsed.ID);
            } else {
                console.error("[LegacyRedirect] No ID found in query string");
            }
        } else {
            console.error("[LegacyRedirect] Unknown path: '" + location.pathname + "'");
        }
    }, [])

    if (path) {
        return <Redirect to={path} />;
    } else {
        return (
            <div id="main">
                {location.pathname}
            </div>
        )
    }
}

export default LegacyRedirect;