import { faWindows } from "@fortawesome/free-brands-svg-icons";
import { useEffect, useState } from "react";
import * as qs from 'query-string';
import { Redirect } from "react-router-dom";

const LegacyRedirect = (propsData) => {
    const [path, setPath] = useState();

    useEffect(() => {
        const parsed = qs.parse(propsData.location.search);

        if (window.location.pathname.toLowerCase() === '/default/single.aspx') {
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
            console.error("[LegacyRedirect] Unknown path: '" + window.location.pathname + "'");
        }
    }, [propsData])

    if (path) {
        return <Redirect to={path} />;
    } else {
        return (
            <div id="main">
                {window.location.pathname}
            </div>
        )
    }
}

export default LegacyRedirect;