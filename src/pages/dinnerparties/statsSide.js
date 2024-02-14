import { useEffect, useState } from "react";
import { MiniPost } from "../../components";
import config from "react-global-configuration";
import axios from "axios";

const StatsSide = ({type}) => {
    const [stats, setStats] = useState();

    useEffect(() => {
        const getStats = async () => {

            var url = new URL(config.get('apiPath'));
            url.pathname = "/api/dinnerparties/statistic";	
            url.searchParams.append("type", type);
            console.log("type", type);
            console.log("url", url.toString());
            try {
                const response = await axios.get(url.toString());
                setStats(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        if(!stats) {
            getStats();
        }
    });
    
    return (
        <div>
            <MiniPost
                title={ type==="winners" ? "Oftast í sigurliði" : "Oftast mætt" }
                description={
                    <ol>
                        {stats ? stats.slice(0,10).map((user) =>
                            <li key={user.id}>{user.username + " (" + user.winCount + " skipti)"}</li>
                        ) : null}
                    </ol>
                }
            />
        </div>
    )
}

export default StatsSide;