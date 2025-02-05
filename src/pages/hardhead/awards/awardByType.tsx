import { useParams } from "react-router-dom";

import { useAward } from '../../../hooks/useAward';
import { Post } from "../../../components";
import AwardsWinners from "./awardsWinners";

const AwardsByType = () => {
    const params = useParams();
    
    if(!params.id) return null;
    
    const { award, error, isLoading } = useAward(parseInt(params.id));

    return (
        <div id="main">
            {isLoading && <div className="loading">Loading award details...</div>}
            {error && <div className="error">Error loading award: {error.message}</div>}
            {award?.years ? award.years.map((year) =>
                        <Post key={year.id}
                            title={`${award.name} ${year.name}`}
                            description={`Harðhausar sem fengu atkvæði: ${year.guestCount}`}
                            body={<AwardsWinners href={award.winners.href} year={year.id} position="" />}
                        />
                    ) : null
            }
        </div>
    )
}

export default AwardsByType;