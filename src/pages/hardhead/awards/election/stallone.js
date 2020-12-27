import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import Post from '../../../../components/post';
import { useAuth } from '../../../../context/auth';

const Stallone = (propsData) => {
    const {authTokens} = useAuth();
    const [users, setUsers] = useState();
    const [savingAllowed, setSavingAllowed] = useState(false);
    const [selectedUser, setSelectedUser] = useState();
    const [text, setText] = useState();

    var url = config.get('path') + '/api/hardhead/' + '5356' + '/users?code=' + config.get('code');

    useEffect(() => {
        const getHardheadUsers = async () => {
            try {
                const response = await axios.get(url);
                setUsers(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        getHardheadUsers();
    }, [propsData, url])

    const handleUserChange = async (event) => {
        if(authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;            
        }

        if(event.target.value !== "") {
            setSelectedUser(event.target.value);
            setSavingAllowed(text !== undefined && event.target.value !== undefined);
        }
        else {
            setSelectedUser(undefined);
            setSavingAllowed(false);
        }        
    }

    const handleTextChange = async (event) => {
        if(authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;            
        }

        if(event.target.value.trim() !== "") {
            setText(event.target.value);
            setSavingAllowed(event.target.value !== undefined && selectedUser !== undefined);
        } else {
            setText(undefined);
            setSavingAllowed(false);            
        }        
        
    }

    const handelSubmit = async (event) => {
        setSavingAllowed(false);
        event.preventDefault();
        if(authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;            
        }

        try {
            var url = config.get('path') + '/api/elections/' + propsData.ID + '/vote?code=' + config.get('code');
            const response = await axios.post(url, {                
                value: selectedUser,
                description: text
            }, {
                headers: {'Authorization': 'token ' + authTokens.token},
            });
        } catch(e) {
            console.error(e);
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
                        <form onSubmit={handelSubmit}>
                            <div className="row gtr-uniform">
                                <div className="col-12">
                                    <select name="demo-category" id="demo-category" onChange={(ev) => handleUserChange(ev)}>
                                        <option value="">- Veldu {propsData.Name} -</option>
                                        {users ?
                                            users.sort((a, b) => a.Name > b.Name ? 1 : -1).map(user =>
                                                <option key={user.ID} value={user.ID}>{user.Name}</option>
                                            ) : null}
                                    </select>
                                </div>
                                <div className="col-12">
                                    <textarea name="demo-message" id="demo-message" placeholder="Skrifaðu rökstuðning fyrir valinu" rows="3" onChange={(ev) => handleTextChange(ev)}></textarea>
                                </div>
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

export default Stallone;