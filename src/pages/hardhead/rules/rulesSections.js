import { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';

const RulesSections = ({id}) => {
    const [data, setData] = useState({ rules: null, isLoading: false, visible: false })

    useEffect(() => {
        const getAwards = async () => {
            var url = config.get('apiPath') + '/api/hardhead/rules/' + id;

            try {
                setData({ isLoading: true });
                const response = await axios.get(url);
                setData({ rules: response.data, isLoading: false, visible: true });
            } catch (e) {
                console.error(e);
                setData({ isLoading: false, visible: false });
            }
        }

        if (!data.rules) {
            getAwards();
        }
    }, [id])

    return (
        <ol>
            {data.visible ?
                data.rules.map((rule) =>
                    <li key={rule.id}>
                        {rule.name}
                    </li>
                ) :
                null}
        </ol>
    )
}

export default RulesSections;