import React, {useState, useEffect} from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import MiniPost from '../../../components/minipost';

// import { MiniListPost } from '../../../components';

const NextHardhead = (propsData) => {
    const[data, setData] = useState({night: null, isLoading: false, visible: false})

    var url = config.get('path') + '/api/hardhead?code=' + config.get('code');		

    useEffect(() => {
        const getNextHardhead = async () => {
            try {
                setData({isLoading: true});

                const response = await axios.get(url);

                if(response.data.length > 0) {
                    setData({night: response.data[0], isLoading: false, visible: true});
                }
            }
            catch(e) {
                console.error(e);
                setData({isLoading: false, visible: false});
            }
        };
        getNextHardhead();
    }, [propsData, url])

    return (
        <div>
            {data.visible ? 
                <MiniPost title="Næsta harðhausakvöld" 
                    description={data.night.Host.Username}
                    dateString={data.night.DateString} 
                    date={data.night.Date}
                    userHref={"/hardhead/users/" + data.night.Host.ID}
                    userPhoto={config.get('imagePath') + data.night.Host.ProfilePhoto.Href + "?code=" + config.get('code')}
                    userText={data.night.Host.Username} /> : null}        
        </div>
    )
}

export default NextHardhead;