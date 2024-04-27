import { useEffect, useState } from "react";
import { MiniPost } from "../../components";

const TotalGuestsSide = ({dinnerParties}) => {
    const [guestCount, setGuestCount] = useState();

    useEffect(() => {
        if(!guestCount && dinnerParties) {
            let count = 0;
            for(const index of Object.keys(dinnerParties)) {
                const party = dinnerParties[index];
                count = count + party.guestCount;           
            }            
            setGuestCount(count);
        }
    });

    return (
        <div>
            <MiniPost
                title="Heildarfjöldi gesta"
                description={guestCount + " hafa samtals mætt á Matar- og Rauðvínskvöld"}
            />
        </div>
    )
};

export default TotalGuestsSide;