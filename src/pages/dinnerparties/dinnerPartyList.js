import SidePost from "../../components/sidepost";

const DinnerPartyList = ({ dinnerParties }) => {

    return (
        <div>
            {dinnerParties ? dinnerParties.map((dinnerParty) =>
                <li key={dinnerParty.id}>
                    <SidePost
                        title={dinnerParty.location}
                        dateString={"Árið " + dinnerParty.year}
                        href={dinnerParty.id}
                    />
                </li>) : null}
        </div>
    )
}

export default DinnerPartyList;