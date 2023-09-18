import { useEffect, useState } from "react"
import config from 'react-global-configuration';
import axios from 'axios';
import { Post } from "../../../components";
import AwardsWinners from "./awardsWinners";
import { useParams } from "react-router-dom";

const AwardsByYear = () => {
    const [categories, setCategories] = useState();
    const params = useParams();

    var url = config.get('path') + '/api/hardhead/awards?year=' + params.id + '&code=' + config.get('code');

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
    }, [params, url])

    return (
        <div id="main">
            {categories ? categories.map((category) =>
                <Post key={category.ID}
                    title={category.Name}
                    description="Efstu sætin þetta ár"
                    body={<AwardsWinners href={category.Winners.Href} year={params.id} position="" />}
                />
            ) : null}
        </div>
    )
}

export default AwardsByYear;