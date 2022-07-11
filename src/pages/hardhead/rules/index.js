import React, {useState, useEffect} from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import { Post } from '../../../components';
import RulesSections from './rulesSections';

const Rules = (propsData) => {
    const[data, setData] = useState({rules: null, isLoading: false, visible: false})

    var url = config.get('path') + '/api/hardhead/rules?code=' + config.get('code');		

    useEffect(() => {
        const getRules = async () => {
            try {
                setData({isLoading: true});
                const response = await axios.get(url);
                setData({rules: response.data, isLoading: false, visible: true});
            } catch(e) {
                console.error(e);
                setData({isLoading: false, visible: false});
            }            
        }

        document.title = "Lög & Reglur Harðhausa | Hress.Org";

        getRules();
    
    }, [propsData, url])

    return (
        <div id="main">
            {data.visible ?
                data.rules.map((rule, i) =>           
                <Post key={rule.ID}
                    id={rule.ID}
                    title={rule.Name}
                    description={(i+1) + ". grein"}
                    body={<RulesSections id={rule.ID}/>}
                />)
            : null}
        </div>
    )
}

export default Rules;