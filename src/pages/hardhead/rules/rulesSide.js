import { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import { MiniPost } from '../../../components';
import axios from 'axios';

const RulesSide = () => {
    const [data, setData] = useState({ rule: null, child: null, isLoading: false, visible: false })

    var url = config.get('apiPath') + '/api/hardhead/rules';

    useEffect(() => {
        var id = 0;

        const getRule = async () => {
            try {
                setData({ isLoading: true });
                const response = await axios.get(url);

                let min = 0;
                let max = response.data.length - 1;
                const random = Math.round(min + Math.random() * (max - min));

                setData({ rule: response.data[random], isLoading: false, visible: true });
                id = response.data[random].id;

                const childUrl = config.get('apiPath') + '/api/hardhead/rules/' + id;
                const childResponse = await axios.get(childUrl);

                min = 0;
                max = childResponse.data.length - 1;
                const childRandom = Math.round(min + Math.random() * (max - min));
                setData({ rule: response.data[random], child: childResponse.data[childRandom], isLoading: false, visible: true });
            } catch (e) {
                console.error(e);
                setData({ isLoading: false, visible: false });
            }
        }

        if (!data.rule) {
            getRule();
        }
    }, [url])

    return (
        <div>
            {data.visible ?
                <MiniPost
                    title="Lög og reglur"
                    href="/hardhead/rules"
                    description={
                        <span>
                            <u>{data.rule.name}</u>
                            <br />
                            {data.child ? data.child.name : null}
                        </span>
                    }
                />
                : null}
        </div>
    )
}

export default RulesSide;