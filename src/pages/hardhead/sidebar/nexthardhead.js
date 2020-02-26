import React, {useState, useEffect} from 'react';
import config from 'react-global-configuration';
import axios from "axios";

// import { MiniListPost } from '../../../components';

const NextHardhead = (propsData) => {
    const[data, setData] = useState({night: null, isLoading: false, visible: false})

    var url = config.get('path') + '/api/hardhead?code=' + config.get('code');		

    useEffect(() => {
        const getNextHardhead = async () => {
            try {
                setData({isLoading: true});

                const response = await axios.get(url);

                console.log("NextHardhead: response='" + response.data + "'");
                console.log("NextHardhead: response_type='" + typeof response.data + "'");

                if(response.data.length > 0) {
                    console.log("NextHardhead: setData");
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
        <article className="mini-post">
            {data.visible ?
                    <header>
                        <h3>Næsta harðhausakvöld</h3>
                        <span className="published">{data.night.Host.Username}</span>
                        <time className="published" dateTime={data.night.Date}>{data.night.DateString}</time>
                        <a href={data.night.Host.Href} className="author"><img src={config.get('path') + data.night.Host.ProfilePhoto.Href + "?code=" + config.get('code')} alt={data.night.Host.Username} /></a>
                    </header>                    
            : null}
        </article>
    )
}

export default NextHardhead;