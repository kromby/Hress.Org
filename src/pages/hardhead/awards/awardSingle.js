import { useEffect, useState } from "react"
import config from 'react-global-configuration';
import axios from "axios";
import { Post } from "../../../components";
import YearsSide from "../components/yearsSide";

const AwardSingle = (propsData) => {
    const [years, setYears] = useState();

    var url = config.get('path') + '/api/hardhead/awards/' + propsData.match.params.id + '?code=' + config.get('code');	

    useEffect(() => {
        const getYears = async () => {
            try {
                const response = await axios.get(url);
                setYears(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        getYears();

    }, [propsData])

    return (
        <div id="main">
            {years ? years.map((year) =>
                <Post key={YearsSide.ID}
                />
            ) : null}
        </div>
    )
}

export default AwardSingle;