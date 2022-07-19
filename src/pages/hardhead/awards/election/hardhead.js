import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import Post from '../../../../components/post';
import { useAuth } from '../../../../context/auth';

const HardheadOfTheYear = (propsData) => {
    const { authTokens } = useAuth();
    const [users, setUsers] = useState();
    const [savingAllowed, setSavingAllowed] = useState(false);
    const [selectedUser, setSelectedUser] = useState();

    var url = config.get('path') + '/api/hardhead/' + '5370' + '/users?attended=8&code=' + config.get('code');

    useEffect(() => {
        const getHardheadUsers = async () => {
            try {
                const response = await axios.get(url);
                setUsers(response.data);
            } catch (e) {
                console.error(e);
                alert(e);
            }
        }

        getHardheadUsers();
    }, [propsData, url])

    const handleUserChange = async (event) => {
        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        var userID = localStorage.getItem("userID");
        if(event == userID) {
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

        try {
            var url = config.get('path') + '/api/elections/' + propsData.ID + '/vote?code=' + config.get('code');
            await axios.post(url, [{
                value: selectedUser
            }], {
                headers: { 'Authorization': 'token ' + authTokens.token },
            });
        } catch (e) {
            console.error(e);
            alert(e);
            setSavingAllowed(true);
        }

        propsData.onSubmit();
    }

    return (
        <Post
            id={propsData.ID}
            title={propsData.Name}
            description={propsData.Description}
            date={propsData.Date}
            dateFormatted={propsData.Year}
            body=
            {
                <section>
                    <form onSubmit={handleSubmit}>
                        <div className="row gtr-uniform">
                            {users ? users.map(user =>
                                <div className="col-4" key={user.ID} onClick={() => handleUserChange(user.ID)} >
                                    <input type="radio" checked={selectedUser === user.ID} onChange={() => handleUserChange(user.ID)} />
                                    <label>
                                        <h3 className="author" width="50%">
                                            {user.ProfilePhoto ?
                                                <img src={config.get("apiPath") + user.ProfilePhoto.Href} alt={user.Name} />
                                                : null}
                                                &nbsp;&nbsp;&nbsp;
                                                <b>{user.Name}</b>
                                        </h3>
                                    </label>
                                    <br />
                                           Mætti á {user.Attended} kvöld<br />
                                    <br />
                                    <br />
                                </div>
                            ) : "null"}
                            <div className="col-12">
                                <ul className="actions">
                                    <li>
                                        <input type="submit" value={"Kjósa " + propsData.Name} disabled={!savingAllowed} />
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </form>
                </section>
            }
        />
    )
}

export default HardheadOfTheYear;