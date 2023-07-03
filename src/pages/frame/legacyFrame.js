import { useEffect, useState } from "react";
import { Redirect } from "react-router-dom";
import iframe from "react-iframe";
import queryString from 'query-string';
import { useAuth } from "../../context/auth"
import config from 'react-global-configuration';
import axios from "axios";

const LegacyFrame = (propsData) => {
    const { authTokens } = useAuth();
    const [isPrivate, setIsPrivate] = useState(false);
    const [isLegacy, setIsLegacy] = useState(false);

    useEffect(() => {
        console.log("[LegacyFrame] Href: '" + window.location.pathname + "'");

        const getMagicCode = async () => {
            try {
                var url = config.get('apiPath') + "/api/authenticate/magic";
                if (authTokens) {
                    const response = await axios.post(url, {}, {
                        headers: { "X-Custom-Authorization": "token " + authTokens.token }
                    });
                    window.location.replace("https://hress.azurewebsites.net/magic/?code=" + response.data + "&path=" + window.location.pathname)
                }
            } catch (e) {
                console.error(e);
            }
        }

        document.title = window.location.pathname + " | Hress.Org";

        const parsed = queryString.parse(propsData.location.search);
        setIsLegacy(parsed.legacy);
        console.log("[LegacyFrame] legacy: '" + parsed.legacy + "'");
        if (parsed.legacy) {
            getMagicCode();
        }
    }, [propsData])

    if (isPrivate && authTokens === undefined) {
        return <Redirect to='/' />
    } else if (isLegacy) {
        return (<div id="main">
            Þú verður fljótlega færð(ur) yfir á á gamla Hressleikann!<br/>
            {"https://hress.azurewebsites.net" + window.location.pathname}
        </div>)
    } else {
        return (<div id="main">
            <iframe
                src={"https://hress.azurewebsites.net/" + window.location.pathname}
                width="100%"
                height="2000px"
                id="myLegacyFrame"
            />
        </div>)
    }
}

export default LegacyFrame;