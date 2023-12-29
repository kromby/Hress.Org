import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import Post from '../../../../components/post';
import { useAuth } from '../../../../context/auth';
import {isMobile} from 'react-device-detect';

const Stallone = ({ID, Name, Description, Date, Year, onSubmit}) => {
    const { authTokens } = useAuth();
    const [stallones, setStallones] = useState();
    const [savingAllowed, setSavingAllowed] = useState(false);
    const [selectedUser, setSelectedUser] = useState();
    const [userID, setUserID] = useState();

    useEffect(() => {

        setUserID(localStorage.getItem("userID"));

        const getNominations = async () => {
            try {
                // var url = config.get('path') + '/api/hardhead/awards/' + ID + '/nominations?code=' + config.get('code');
                var url = config.get('apiPath') + '/api/hardhead/awards/nominations?type=' + ID;
                const response = await axios.get(url, {
                    headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
                });
                setStallones(response.data);
            } catch (e) {
                console.error(e);
                alert(e);
            }
        }

        if (!stallones) {
            getNominations();
        }
    }, [ID])

    const handleChange = async (event) => {
        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        var userID = localStorage.getItem("userID");
        if (event == userID) {
            alert("Ætlar þú í alvöru að kjósa sjálfan þig, það er ekki mjög Harðhausalegt.");
            return;
        }


        setSelectedUser(event);
        setSavingAllowed(true);
    }

    const handleSubmit = async (event) => {
        setSavingAllowed(false);
        event.preventDefault();
        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        var voteData = [{ id: selectedUser, Value: stallones.filter(n => n.id === selectedUser)[0].nominee.id }];

        try {
            var url = config.get('apiPath') + '/api/elections/' + ID + '/vote';
            await axios.post(url, voteData, {
                headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
            });
        } catch (e) {
            console.error(e);
            alert(e);
            setSavingAllowed(true);
        }

        onSubmit();
    }

    return ([
        <Post key="1"
            id={ID}
            title={Name}
            description={Description}
            date={Date}
            dateFormatted={Year}
            body={
                <section>
                    <div className="row gtr-uniform">
                        {stallones ? stallones.map(stallone =>
                            <div className={isMobile ? "col-12" : "col-6"} key={stallone.id} onClick={() => handleChange(stallone.id)} >
                                <input type="radio" checked={selectedUser === stallone.id} onChange={() => handleChange(stallone.id)} />
                                <label>
                                    <h3 className="author" width="50%">
                                        {stallone.nominee.profilePhoto ?
                                            <img src={config.get("apiPath") + stallone.nominee.profilePhoto.href} alt={stallone.nominee.name} />
                                            : null}
                                        &nbsp;&nbsp;&nbsp;
                                        <b>{stallone.nominee.name}</b>
                                    </h3>
                                </label>
                                <br />
                                {stallone.description}
                                <br />
                                <br />
                            </div>
                        ) : null}
                    </div>
                </section>
            }
        />,
        <ul key="2" className="actions pagination">
            <li>
                <button onClick={handleSubmit} disabled={!savingAllowed} className="button large next">{"Kjósa " + Name}</button>
            </li>
        </ul>
    ])
}

export default Stallone;