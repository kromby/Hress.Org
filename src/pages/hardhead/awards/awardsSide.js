import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import { MiniPost } from '../../../components';
import axios from 'axios';

const AwardsSide = (propsData) => {
    const [data, setData] = useState({ awards: null, isLoading: false, visible: false })

    var url = config.get('path') + '/api/hardhead/awards/364/winners?position=1&code=' + config.get('code');

    useEffect(() => {
        const getAwards = async () => {
            try {
                setData({ isLoading: true });
                const response = await axios.get(url);
                setData({ awards: response.data[0], isLoading: false, visible: true });
            } catch (e) {
                console.error(e);
                setData({ isLoading: false, visible: false });
            }
        }

        if (!data.awards) {
            getAwards();
        }
    }, [propsData, url])

    return (
        <div>
            {data.visible && data.awards ?
                <MiniPost title="Harðhausa verðlaunin" href="/hardhead/awards"
                    description={<span>Harðhaus ársins<br />{data.awards.Winner.Username} með {data.awards.Value} atkvæði</span>}
                    date={"1.1." + data.awards.Year}
                    dateString={data.awards.Year}
                    userHref={"/hardhead/users/" + data.awards.Winner.ID}
                    userPhoto={config.get('path') + data.awards.Winner.ProfilePhoto.Href + "?code=" + config.get('code')}
                    userText={data.awards.Winner.Username} /> :
                null}
        </div>
    )
}

export default AwardsSide;