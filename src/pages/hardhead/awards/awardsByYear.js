import { useEffect, useState } from "react"
import config from 'react-global-configuration';
import axios from 'axios';
import { Post } from "../../../components";
import AwardsWinners from "./awardsWinners";

const AwardsByYear = (propsData) => {
    const [categories, setCategories] = useState();

    var url = config.get('path') + '/api/hardhead/awards?year=' + propsData.match.params.id + '&code=' + config.get('code');

    useEffect(() => {
        const getAwardsByYear = async () => {
            try {
                const response = await axios.get(url);
                setCategories(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        if (!categories) {
            getAwardsByYear();
        }
    }, [propsData, url])

    return (
        <div id="main">
            {categories ? categories.map((category) =>
                <Post key={category.ID}
                    title={category.Name}
                    description="Efstu sætin þetta ár"
                    body={<AwardsWinners href={category.Winners.Href} year={propsData.match.params.id} position="" />}
                />
            ) : null}
        </div>
    )
}

export default AwardsByYear;