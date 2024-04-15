import { useEffect, useState } from "react";
import queryString from 'query-string';
import { useAuth } from "../../context/auth"
import config from 'react-global-configuration';
import axios from "axios";
import { useLocation, useNavigate } from "react-router-dom";

const LegacyFrame = () => {
    const { authTokens } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();
    const [isPrivate] = useState(false);
    const [isLegacy, setIsLegacy] = useState(false);    

    useEffect(() => {
        

        if (isPrivate && authTokens === undefined) {
            navigate("/login", { state: { from: location.pathname } });
            return;
        }

        const getMagicCode = async () => {
            try {
                var url = config.get('apiPath') + "/api/authenticate/magic";
                if (authTokens) {
                    const response = await axios.post(url, {}, {
                        headers: { "X-Custom-Authorization": "token " + authTokens.token }
                    });                    
                    window.location.replace("https://hress.azurewebsites.net/magic/?code=" + response.data + "&path=" + location.pathname);
                }
            } catch (e) {
                console.error(e);
            }
        }

        document.title = location.pathname + " | Hress.Org";

        const parsed = queryString.parse(location.search);
        setIsLegacy(parsed.legacy);
        
        if (parsed.legacy) {
            getMagicCode();
        }
    }, [])

    if (isLegacy) {
        return (<div id="main">
            Þú verður fljótlega færð(ur) yfir á á gamla Hressleikann!<br />
            {"https://hress.azurewebsites.net" + location.pathname}
        </div>)
    } else {
        return (<div id="main">
            <iframe
                src={"https://hress.azurewebsites.net/" + location.pathname}
                width="100%"
                height="2000px"
                id="myLegacyFrame"
            />
        </div>)
    }
}

export default LegacyFrame;