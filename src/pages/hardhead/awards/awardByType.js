import { useEffect, useState } from "react"
import config from 'react-global-configuration';
import axios from "axios";
import { Post } from "../../../components";
import YearsSide from "../components/yearsSide";
import AwardsWinners from "./awardsWinners";
import { useParams } from "react-router-dom-v5-compat";

const AwardsByType = () => {
    const [award, setAward] = useState();
    const params = useParams();

    var url = config.get('path') + '/api/hardhead/awards/' + params.id + '?code=' + config.get('code');

    useEffect(() => {
        const getYears = async () => {
            try {
                const response = await axios.get(url);
                setAward(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        if (!award) {
            getYears();
        }
    }, [])

    return (
        <div id="main">
            {award && award.Years ? award.Years.map((year) =>
                <Post key={year.ID}
                    title={award.Name + " " + year.Name}
                    description={"Harðhausar sem fengu atkvæði: " + year.GuestCount}
                    body={<AwardsWinners href={award.Winners.Href} year={year.ID} position="" />}
                />
            ) : null}
        </div>
    )
}

export default AwardsByType;