import React, { useEffect, useState } from 'react';
import { Redirect } from 'react-router-dom'
import config from 'react-global-configuration';
import axios from "axios";
import { useAuth } from '../../../../context/auth';
import StalloneNomination from './stalloneNomination';
import DisappointmentNomination from './disappointmentNomination';

const Nominations = (propsData) => {
    const { authTokens } = useAuth();
    const [users, setUsers] = useState();

    var url = config.get('path') + '/api/hardhead/5384/users?code=' + config.get('code');

    useEffect(() => {
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
    }, [propsData, url])

    if (authTokens === undefined) {
        return <Redirect to='/hardhead' />
    }
    else {
        return (
            <div id="main">
                <StalloneNomination Users={users} Type="5284" />
                <DisappointmentNomination Users={users} Type="360" />
            </div>
        )
    }
}

export default Nominations;