import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import { useAuth } from '../../../context/auth';
import UserImage from '../../../components/users/userimage';
import { useLocation } from 'react-router-dom-v5-compat';

const GuestsEdit = ({ hardheadID, users }) => {
    const { authTokens } = useAuth();
    const location = useLocation();
    const [guests, setGuests] = useState();

    const getGuests = async () => {
        var url = config.get('path') + '/api/hardhead/' + hardheadID + '/guests?code=' + config.get('code')
        axios.get(url)
            .then(response => {
                setGuests(response.data);
            })
            .catch(error => {
                if (error.response.status === 404) {
                    console.log("[GuestsEdit] Guests not found for Hardhead: " + hardheadID);
                } else {
                    console.error("[GuestsEdit] Error retrieving guests for Hardhead: " + hardheadID);
                    console.error(error);
                }
            })
    }

    useEffect(() => {
        if (authTokens === undefined) {
            return <Redirect to={{ pathname: "/login", state: { from: location.pathname } }} />
        }

        if (!guests) {
            getGuests();
        }
    }, [hardheadID, authTokens])

    const handleGuestChange = async (event) => {
        if (authTokens !== undefined) {
            event.preventDefault();
            try {
                var guestID = event.target.value;
                var url = config.get('path') + '/api/hardhead/' + hardheadID + '/guests/' + guestID + '?code=' + config.get('code');
                const response = await axios.post(url, {}, {
                    headers: { 'Authorization': 'token ' + authTokens.token },
                });
                getGuests();

                setUsers(users.filter(u => {
                    return u.ID != guestID;
                }));
            } catch (e) {
                console.error("[GuestsEdit] Ekki tókst að bæta gest við.");
                console.error(e);
            }
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