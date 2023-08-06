import React, {useState, useEffect} from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import MiniPost from '../../../components/minipost';
import HardheadActions from './actions';

const NextHardhead = (propsData) => {
    const[hardhead, setHardhead] = useState();
    const[allowEdit, setAllowEdit] = useState(false);

    var url = config.get('path') + '/api/hardhead?code=' + config.get('code');		

    useEffect(() => {
        const getNextHardhead = async () => {
            try {
                const response = await axios.get(url);

                if(response.data.length > 0) {
                    setHardhead(response.data[0]);
                }
            }
            catch(e) {
                console.error(e);
            }
        };

        setAllowEdit(propsData.allowEdit);

        if(!hardhead) {
            getNextHardhead();
        }
    }, [propsData, url])

    return (
        <div>
            {hardhead ? 
                <MiniPost title="Næsta harðhausakvöld" 
                    description={<span>{hardhead.Host.Username}<br/><br/>{allowEdit ? <HardheadActions id={hardhead.ID}/>:null}</span>}
                    dateString={hardhead.DateString} 
                    date={hardhead.Date}
                    userHref={"/hardhead/users/" + hardhead.Host.ID}
                    userPhoto={config.get('apiPath') + hardhead.Host.ProfilePhoto.Href}
                    userText={hardhead.Host.Username} /> : null}        
        </div>
    )
}

export default NextHardhead;