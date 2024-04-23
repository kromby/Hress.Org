import { useEffect, useState } from 'react';
import config from 'react-global-configuration';
import { useAuth } from '../../../context/auth';
import axios from "axios";

const HardheadActions = ({id}) => {
    const { authTokens } = useAuth();
    const [data, setData] = useState({ actions: [], isLoading: false, visible: false });
    const [lastLoggedIn, setLastLoggedIn] = useState(false);

    useEffect(() => {
        const getActions = async () => {
            if (authTokens !== undefined) {
                try {
                    var url = `${config.get('apiPath')}/api/hardhead/${id}/actions`;
                    const response = await axios.get(url, {
                        headers: { 'X-Custom-Authorization': 'token ' + authTokens.token }
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
    }, [id, authTokens])

    return (
        <ul className="actions">
            {data.visible ?
                data.actions.map(action =>
                    <li key={action.link.href}>
                        <a href={action.link.href} title={action.description} className="button large">{action.name}</a>
                    </li>) : null
            }
        </ul>
    )
}

export default HardheadActions;