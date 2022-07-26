import React, { useEffect, useState } from 'react';
import config from 'react-global-configuration';
import { useAuth } from '../../../context/auth';
import axios from "axios";

const HardheadActions = (propsData) => {
    const { authTokens } = useAuth();
    const [data, setData] = useState({ actions: [], isLoading: false, visible: false });
    const [lastLoggedIn, setLastLoggedIn] = useState(false);

    useEffect(() => {
        const getActions = async () => {
            if (authTokens !== undefined) {
                try {
                    var url = config.get('path') + '/api/hardhead/' + propsData.id + '/actions?code=' + config.get('code');
                    const response = await axios.get(url, {
                        headers: { 'Authorization': 'token ' + authTokens.token }
                    })
                    setData({ actions: response.data, isLoading: false, visible: true })
                }
                catch (e) {
                    console.error(e);
                    setData({ isLoading: false, visible: false });
                }
                //setData({visible: true});
            }
        }

        var loggedIn = authTokens ? true : false;

        if (!data.actions || lastLoggedIn != loggedIn) {
            getActions();
            setLastLoggedIn(loggedIn);
        }
    }, [propsData, authTokens])

    return (
        <ul className="actions">
            {data.visible ?
                data.actions.map(action =>
                    <li key={action.Link.Href}>
                        <a href={action.Link.Href} tooltip={action.Description} className="button large">{action.Name}</a>
                    </li>) : null
            }
        </ul>
    )
}

export default HardheadActions;