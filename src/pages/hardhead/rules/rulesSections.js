import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';

const RulesSections = (propsData) => {
    const [data, setData] = useState({ rules: null, isLoading: false, visible: false })

    useEffect(() => {
        const getAwards = async () => {
            var url = config.get('apiPath') + '/api/hardhead/rules/' + propsData.id;

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
    }, [propsData])

    return (
        <ol>
            {data.visible ?
                data.rules.map((rule, i) =>
                    <li key={rule.id}>
                        {rule.name}
                    </li>
                ) :
                null}
        </ol>
    )
}

export default RulesSections;