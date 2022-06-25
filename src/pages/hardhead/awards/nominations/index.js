import React, { useEffect, useState } from 'react';
import { Redirect } from 'react-router-dom'
import { Post } from '../../../../components';
import config from 'react-global-configuration';
import axios from "axios";
import { useAuth } from '../../../../context/auth';

const Nominations = (propsData) => {
    const { authTokens } = useAuth();
    const [buttonEnabled, setButtonEnabled] = useState(false);
    const [users, setUsers] = useState();
    const [nominations, setNominations] = useState();
    const [description, setDescription] = useState();
    const [nominee, setNominee] = useState();
    const [isSaved, setIsSaved] = useState(false);
    const [error, setError] = useState();

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

        const getNominations = async () => {
            try {
                var getUrl = config.get('apiPath') + '/api/hardhead/awards/nominations?type=207';
                const response = await axios.get(getUrl, { headers: { 'X-Custom-Authorization': 'token ' + authTokens.token } });
                setNominations(response.data);

            } catch (e) {
                console.error(e);
            }
        }

        getUsers();
    }, [propsData, url])

    const handleSubmit = async (event) => {
        setButtonEnabled(false);

        //if (authTokens === undefined) {
        event.preventDefault();

        try {
            var postUrl = config.get('apiPath') + '/api/hardhead/awards/nominations';
            const response = await axios.post(postUrl, {
                typeID: 207,
                description: description,
                nomineeID: nominee
            }, {
                headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
            });
            console.log(response);
            setIsSaved(true);
        } catch (e) {            
            console.error(e); 
            if(e.response && e.response.status === 400) {
                setError("Ekki tókst að skrá tilnefningu! - " + e.message);
            }
            else {
                setError("Ekki tókst að skrá tilnefningu!");
            }
        }

        getNominations();
    }

    const handleNomineeChange = (event) => { setNominee(event.target.value); setButtonEnabled(allowSaving(event.target.value, description)); }
    const handleDescriptionChange = (event) => { setDescription(event.target.value); setButtonEnabled(allowSaving(nominee, event.target.value)); }

    const allowSaving = (nomineeID, descriptionText) => {
        if (descriptionText === undefined)
            return false;
        if (descriptionText.length <= 10 || nomineeID.length <= 0) {
            return false;
        }

        setIsSaved(false);
        setError("");
        console.log("nomineeID: " + nomineeID);
        return true;
    }

    if (authTokens === undefined) {
        return <Redirect to='/hardhead' />
    }
    else {
        return (
            <div id="main">
                <Post
                    title="Stallone ársins"
                    description="Tilnefndu Harðhaus fyrir frábært afrek"
                    body={
                        <form onSubmit={handleSubmit}>
                            <div className="row gtr-uniform">
                                <div className="col-6 col-12-xsmall">
                                    {users ?
                                        <select id="demo-category" name="demo-category" onChange={(ev) => handleNomineeChange(ev)}>
                                            <option value="">- Hvaða Harðhaus vilt þú tilnefna? -</option>
                                            {users.sort((a, b) => a.Name.toLowerCase() > b.Name.toLowerCase() ? 1 : -1).map(user =>
                                                <option key={user.ID} value={user.ID}>
                                                    {user.Name}
                                                </option>
                                            )}
                                        </select>
                                        : null}
                                </div>
                                <div className="col-12">
                                    <textarea name="Lýsing" rows="3" onChange={(ev) => handleDescriptionChange(ev)} defaultValue={description} placeholder="Fyrir hvað vilt þú tilnefna?" />
                                </div>
                                <div className="col-12">
                                    {isSaved ? <b>Tilnefning skráð!<br /></b> : null}
                                    {error ? <b>{error}<br /></b> : null}
                                    <button tooltip="Tilnefna" className="button large" disabled={!buttonEnabled}>Tilnefna</button>
                                </div>
                            </div>
                        </form>
                    }
                />
            </div>
        )
    }
}

export default Nominations;