import { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import { Post } from '../../../components';
import RulesSections from './rulesSections';

const Rules = () => {
    const [data, setData] = useState({ rules: null, isLoading: false, visible: false })    

    useEffect(() => {
        const getRules = async () => {
            try {
                const url = `${config.get('apiPath')}/api/hardhead/rules`;
                setData({ isLoading: true });
                const response = await axios.get(url);
                setData({ rules: response.data, isLoading: false, visible: true });
            } catch (e) {
                console.error(e);
                setData({ isLoading: false, visible: false });
            }
        }

        document.title = "Lög & Reglur Harðhausa | Hress.Org";

        if (!data.rules) {
            getRules();
        }

    }, [])

    return (
        <div id="main">
            {data.visible ?
                data.rules.map((rule, i) =>
                    <Post key={rule.id}
                        id={rule.id}
                        title={rule.name}
                        description={(i + 1) + ". grein"}
                        body={<RulesSections id={rule.id} />}
                    />)
                : null}
        </div>
    )
}

export default Rules;