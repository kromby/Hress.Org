import React, {useState, useEffect} from 'react';
import config from 'react-global-configuration';
import axios from 'axios';

const RulesSections = (propsData) => {
    const[data, setData] = useState({rules: null, isLoading: false, visible: false})    		

    useEffect(() => {
        const getAwards = async () => {
            var url = config.get('path') + '/api/hardhead/rules/' + propsData.id + '?code=' + config.get('code');

            try {
                setData({isLoading: true});
                const response = await axios.get(url);
                setData({rules: response.data, isLoading: false, visible: true});
            } catch(e) {
                console.error(e);
                setData({isLoading: false, visible: false});
            }
        }

        getAwards();

    }, [propsData])    

    return (
        <ol>
            {data.visible ?
                data.rules.map((rule, i) =>                 
                    <li key={rule.ID}>
                        {rule.Name}
                    </li>
                ) : 
            null}
        </ol>
    )
}

export default RulesSections;