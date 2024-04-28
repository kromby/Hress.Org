import UserAwards from "./userAwards";
import { useLocation, useParams } from "react-router-dom";
import config from 'react-global-configuration';
import UserStatistics from "./userStatistics";
import Challenge from "./challenge";
import axios from "axios";
import { useState, useEffect } from "react";
import Streak from "./streak";

const HHUsers = () => {
    const params = useParams();
    const location = useLocation();
    const query = new URLSearchParams(location.search);
    const flag = query.get("flag");
    const [user, setUser] = useState();

    useEffect(() => {
        const getUser = async () => {
            const url = config.get('path') + '/api/users/' + params.id + '/?code=' + config.get('code');
            try {
                const response = await axios.get(url);
                setUser(response.data);
                document.title = response.data.Name + " | Hress.Org";
            } catch (e) {
                console.error(e);
            }
        }        

        if (!user) {
            getUser();
        }
    }, [])

    return (
        <div id="main">
            {flag !== "new" ? [
            <UserAwards key="one" id={params.id} />,
            <UserStatistics key="two" id={params.id} />
            ] : null}
            { user ? <Challenge id={params.id} username={user.Name} profilePhoto={user.ProfilePhoto?.Href} /> : null}            
            {flag === "new" || flag === "all" ? <Streak id={params.id} /> : null}
        </div>
    )
}

export default HHUsers;