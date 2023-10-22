import { MiniPost } from "../../components";

const GuestCountSide = ({ dinnerParties }) => {
    return (
        <div>
            <MiniPost
                title="Flestir gestir"
                description={
                    <ol>
                        {dinnerParties ? dinnerParties.sort((a, b) => a.guestCount < b.guestCount ? 1 : -1).slice(0,5).map((dinnerParty) =>
                            <li>{"Árið " + dinnerParty.year + " " + dinnerParty.location + " - Gestir: " + dinnerParty.guestCount}</li>
                        ) : null}                        
                    </ol>
                }
            />
        </div>
    )
}

export default GuestCountSide;