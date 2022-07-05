import { useEffect, useState } from "react";
import axios from "axios";
import config from 'react-global-configuration';
import { Post } from "../../../components";
import UserAwardDetail from "./userAwardDetail";

const UserAwards = (propsData) => {
    const [awards, setAwards] = useState();

    useEffect(() => {
        const getAwards = async () => {
            var url = config.get('path') + '/api/hardhead/awards?code=' + config.get('code');

            try {
                const response = await axios.get(url);
                setAwards(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        if(!awards) {
            getAwards();
        }
    }, [propsData])

    return (
        <Post key={null}
            id={null}
            title="Verðlaunin"
            description="Efstu þrjú sætin"
            body={
                <section>
                    {awards ? awards.map(award => <UserAwardDetail key={award.ID} awardID={award.ID} name={award.Name} userID={propsData.id} />) : null}
                </section>
            }
        />
    );
}

export default UserAwards;