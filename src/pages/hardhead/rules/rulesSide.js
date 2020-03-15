import React, {useState, useEffect} from 'react';
import config from 'react-global-configuration';
import { MiniPost } from '../../../components';
import axios from 'axios';

const RulesSide = (propsData) => {
    const[data, setData] = useState({rule: null, child: null, isLoading: false, visible: false})

    var url = config.get('path') + '/api/hardhead/rules?code=' + config.get('code');		

    useEffect(() => {
        var id = 0;

        const getRule = async () => {
            try {
                setData({isLoading: true});
                const response = await axios.get(url);

                var min = 0;
                var max = response.data.length-1;            
                const random = Math.round(min + Math.random() * (max - min));

                setData({rule: response.data[random], isLoading: false, visible: true});
                id = response.data[random].ID;

                var childUrl = config.get('path') + '/api/hardhead/rules/' + id + '?code=' + config.get('code');		
                const childResponse = await axios.get(childUrl);

                min = 0;
                max = childResponse.data.length-1;
                const childRandom = Math.round(min + Math.random() * (max - min));
                setData({rule: response.data[random], child: childResponse.data[childRandom], isLoading: false, visible: true});
            } catch(e) {
                console.error(e);
                setData({isLoading: false, visible: false});
            }            
        }

        getRule();
    
    }, [propsData, url])

    return (
        <div>
            {data.visible ?
            <MiniPost 
                title="Lög og reglur" 
                href="http://www.hress.org/Hardhead/Rules.aspx"
                description={
                    <span>
                        <u>{data.rule.Name}</u>
                        <br/>
                        {data.child ? data.child.Name : null}
                    </span>
                } 
            /> 
            : null}
        </div>
    )
}

export default RulesSide;