import React, { useEffect, useState } from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import { useAuth } from '../../../../context/auth';
import StalloneNomination from './stalloneNomination';
import DisappointmentNomination from './disappointmentNomination';
import { useLocation } from 'react-router-dom';

const Nominations = () => {
    const { authTokens } = useAuth();
    const location = useLocation();
    const [users, setUsers] = useState();

    var url = config.get('path') + '/api/hardhead/5384/users?code=' + config.get('code');

    useEffect(() => {
        if (authTokens === undefined) {
            navigate("/login", { state: { from: location.pathname } });
            return;
        }

        const getUsers = async () => {
            try {
                var userID = localStorage.getItem("userID");
                const response = await axios.get(url);
                setUsers(response.data.filter(user => user.ID != userID));
            } catch (e) {
                console.error(e);
            }
        }

        document.title = "Tilnefningar | Hress.Org";

        if (!users) {
            getUsers();
        }
    }, [url])

    return (
        <div id="main">
            <StalloneNomination Users={users} Type="5284" />
            <DisappointmentNomination Users={users} Type="360" />
        </div>
    )
}

export default Nominations;