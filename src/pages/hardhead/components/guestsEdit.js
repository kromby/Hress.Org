import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import { useAuth } from '../../../context/auth';
import UserImage from '../../../components/users/userimage';

const GuestsEdit = (propsData) => {
    const { authTokens } = useAuth();
    const [guests, setGuests] = useState();
    const [users, setUsers] = useState();

    const getGuests = async () => {
        try {
            var url = config.get('path') + '/api/hardhead/' + propsData.hardheadID + '/guests?code=' + config.get('code')
            const response = await axios.get(url);
            setGuests(response.data);
        } catch (e) {
            console.error(e);
        }
    }

    useEffect(() => {
        if (authTokens === undefined) {
            // TODO Redirect back to main page
        }

        setUsers(propsData.users);

        if (!guests) {
            getGuests();
        }
    }, [propsData, authTokens])

    const handleGuestChange = async (event) => {
        if (authTokens !== undefined) {
            event.preventDefault();
            try {
                console.log(propsData.hardheadID);
                var guestID = event.target.value;
                console.log(guestID);
                var url = config.get('path') + '/api/hardhead/' + propsData.hardheadID + '/guests/' + guestID + '?code=' + config.get('code');
                const response = await axios.post(url, {}, {
                    headers: { 'Authorization': 'token ' + authTokens.token },
                });
                getGuests();

                setUsers(users.filter(u => {
                    return u.ID != guestID;
                }));

                console.log(response);
            } catch (e) {
                console.error(e);
                alert("Ekki tókst að bæta gest við.");
            }
        } else {
            // TODO: redirect to main page
        }
    }

    return (
        <section>
            <h3>Gestir</h3>
            <div className="row gtr-uniform">
                <div className="col-12">
                    {users ?
                        <select id="demo-category" name="demo-category" onChange={(ev) => handleGuestChange(ev)}>
                            <option value="">- Veldu gest? -</option>
                            {users.sort((a, b) => a.Name > b.Name ? 1 : -1).map(user =>
                                <option key={user.ID} value={user.ID}>
                                    {user.Name}
                                </option>
                            )}
                        </select>
                        : null}
                </div>
                {guests ?
                    guests.map(guest =>
                        <div className="col-2 col-12-xsmall align-center" key={guest.ID}>
                            {typeof guest.ProfilePhoto !== 'undefined' ?
                                <UserImage id={guest.ID} username={guest.Username} profilePhoto={guest.ProfilePhoto.Href} /> :
                                <UserImage id={guest.ID} username={guest.Username} />
                            }
                        </div>
                    )
                    : "Enginn skráður gestur"}
            </div>
        </section>
    )
}

export default GuestsEdit;