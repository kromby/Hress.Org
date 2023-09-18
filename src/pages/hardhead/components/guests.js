import React, { Component, useEffect, useState } from 'react';
import config from 'react-global-configuration';
import UserImage from '../../../components/users/userimage';
import axios from "axios";

const Guests = ({ hardheadID }) => {
    const [error, setError] = useState();
    const [guests, setGuests] = useState();

    useEffect(() => {
        const getGuests = async () => {
            var url = config.get('path') + '/api/hardhead/' + hardheadID + '/guests?code=' + config.get('code');

            axios.get(url)
                .then((response) => setGuests(response.data))
                .catch((ex) => {
                    if (ex.response.status !== 404) {
                        setError(ex);
                        console.log("[Guests] Error retrieving guests");
                    }
                })
        }

        if (!guests) {
            getGuests();
        }
    }, [hardheadID])

    return (
        <section>
            <h3>Gestir</h3>
            <div className="row gtr-uniform">
                {guests ? guests.map(guest =>
                    <div className="col-2 align-center" key={guest.ID}>
                        {typeof guest.ProfilePhoto !== 'undefined' ?
                            <UserImage id={guest.ID} username={guest.Username} profilePhoto={guest.ProfilePhoto.Href} /> :
                            <UserImage id={guest.ID} username={guest.Username} />
                        }
                    </div>
                ) : null}
                {error ? error : null}
            </div>
        </section>
    )
}

export default Guests;