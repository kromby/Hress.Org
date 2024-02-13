import { useEffect, useState } from "react";
import { MiniPost } from "../../components";
import config from "react-global-configuration";
import axios from "axios";

const WinnerStatsSide = () => {
    const [winners, setWinners] = useState();

    useEffect(() => {
        const getWinners = async () => {
            var url = config.get('apiPath') + '/api/dinnerparties/statistic?type=winners';
            try {
                const response = await axios.get(url);
                setWinners(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        if(!winners) {
            getWinners();
        }
    });
    
    return (
        <div>
            <MiniPost
                title="Oftast sigurvegari"
                description={
                    <ol>
                        {winners ? winners.slice(0,10).map((winner) =>
                            <li key={winner.id}>{winner.username + " (" + winner.winCount + " sigrar)"}</li>
                        ) : null}
                    </ol>
                }
            />
        </div>
    )
}

export default WinnerStatsSide;