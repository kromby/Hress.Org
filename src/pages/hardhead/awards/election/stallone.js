import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import Post from '../../../../components/post';
import { useAuth } from '../../../../context/auth';

const Stallone = (propsData) => {
    const { authTokens } = useAuth();
    const [stallones, setStallones] = useState();
    const [savingAllowed, setSavingAllowed] = useState(false);
    const [selectedUser, setSelectedUser] = useState();
    const [userID, setUserID] = useState();

    var url = config.get('path') + '/api/hardhead/5384/users?code=' + config.get('code');

    useEffect(() => {

        setUserID(localStorage.getItem("userID"));

        const getNominations = async () => {
            try {
                var url = config.get('path') + '/api/hardhead/awards/' + propsData.ID + '/nominations?code=' + config.get('code');
                const response = await axios.get(url);
                setStallones(response.data);
            } catch (e) {
                console.error(e);
                alert(e);
            }
        }

        if (!stallones) {
            getNominations();
        }
    }, [propsData, url])

    const handleChange = async (event) => {
        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
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

        var voteData = [{ PollEntryID: selectedUser, Value: stallones.filter(n => n.ID === selectedUser)[0].Nominee.ID }];

        try {
            var url = config.get('apiPath') + '/api/elections/' + propsData.ID + '/vote';
            await axios.post(url, voteData, {
                headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
            });
        } catch (e) {
            console.error(e);
            alert(e);
            setSavingAllowed(true);
        }

        propsData.onSubmit();
    }

    return ([
        <Post key="1"
            id={propsData.ID}
            title={propsData.Name}
            description={propsData.Description}
            date={propsData.Date}
            dateFormatted={propsData.Year}
            body={
                <section>
                    <div className="row gtr-uniform">
                        {stallones ? stallones.map(stallone =>
                            <div className="col-6" key={stallone.ID} onClick={() => handleChange(stallone.ID)} >
                                <input type="radio" checked={selectedUser === stallone.ID} onChange={() => handleChange(stallone.ID)} />
                                <label>
                                    <h3 className="author" width="50%">
                                        {stallone.Nominee.ProfilePhoto ?
                                            <img src={config.get("apiPath") + stallone.Nominee.ProfilePhoto.Href} alt={stallone.Nominee.Username} />
                                            : null}
                                        &nbsp;&nbsp;&nbsp;
                                        <b>{stallone.Nominee.Username}</b>
                                    </h3>
                                </label>
                                <br />
                                {stallone.Name}
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
                <button onClick={handleSubmit} disabled={!savingAllowed} className="button large next">{"Kjósa " + propsData.Name}</button>
            </li>
        </ul>
    ])
}

export default Stallone;