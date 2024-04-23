import { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import { Post } from '../../../components';
import AwardsWinners from './awardsWinners';

const Awards = () => {
    const [data, setData] = useState({ awards: null, isLoading: false, visible: false })

    const url = config.get('path') + '/api/hardhead/awards?code=' + config.get('code');

    useEffect(() => {
        const getAwards = async () => {
            try {
                setData({ isLoading: true });
                const response = await axios.get(url);
                setData({ awards: response.data, isLoading: false, visible: true });
            } catch (e) {
                console.error(e);
                setData({ isLoading: false, visible: false });
            }
        }

        document.title = "Harðhausaverðlaunin | Hress.Org";

        if (!data.awards) {
            getAwards();
        }
    }, [url])

    return (
        <div id="main">
            {data.visible ?
                data.awards.map((award) =>
                    <Post key={award.ID}
                        id={award.ID}
                        href={"/hardhead/awards/" + award.ID}
                        title={award.Name}
                        description="Sigurvegarar frá upphafi"
                        body={<AwardsWinners href={award.Winners.Href} position="1" />}
                    />
                ) :
                null}
        </div>
    )
}

export default Awards;