import { useEffect, useState } from "react";
import { MiniPost } from "../../components";

const GuestCountSide = ({ dinnerParties }) => {
    const[parties, setParties] = useState();

    useEffect(() => {
        if(!parties) {
            setParties(dinnerParties);
        }
    })
    return (
        <div>
            <MiniPost
                title="Flestir gestir"
                description={
                    <ol>
                        {parties ? parties.sort((a, b) => a.guestCount < b.guestCount ? 1 : -1).slice(0,5).map((dinnerParty) =>
                            <li key={dinnerParty.id}>{"Árið " + dinnerParty.year + " " + dinnerParty.location + " - Gestir: " + dinnerParty.guestCount}</li>
                        ) : null}                        
                    </ol>
                }
            />
        </div>
    )
}

export default GuestCountSide;