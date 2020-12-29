import React, {useState, useEffect} from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import { useAuth } from '../../../context/auth';

const GuestsEdit = (propsData) => {    
    const {authTokens} = useAuth();
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
		if(authTokens === undefined) {
			// TODO Redirect back to main page
        }


        
        setUsers(propsData.users);

		getGuests();
    }, [propsData, authTokens])

    const handleGuestChange = async (event) => { 
        if(authTokens !== undefined){			
			event.preventDefault();			
			try {
                console.log(propsData.hardheadID);
                var guestID = event.target.value;
                console.log(guestID);
				var url = config.get('path') + '/api/hardhead/' + propsData.hardheadID + '/guests/' + guestID + '?code=' + config.get('code');
				const response = await axios.post(url, {}, {
					headers: {'Authorization': 'token ' + authTokens.token},            
				});
                getGuests();

                setUsers(users.filter(u => {
                    return u.ID != guestID;
                }));

				console.log(response);
			} catch(e) {
				console.error(e);
				alert("Ekki tókst að bæta gest við.");
			}
		} else {
			// TODO: redirect to main page
		}
    }

    return (
        <div>
            <h3>Gestir</h3>
                {users ? 
                    <select id="demo-category" name="demo-category" onChange={(ev) => handleGuestChange(ev)}>
                        <option value="">- Veldu gest? -</option>
                    {users.sort((a,b) => a.Name > b.Name ? 1 : -1).map(user => 
                        <option key={user.ID} value={user.ID}>
                            {user.Name}
                        </option>
                    )}
                    </select>
                : null}	
                <br/>
                <ul>
                    {guests ? 
                        guests.map(guest =>
                        <li key={guest.ID}>{guest.Username}</li> 
                        )
                        : <b>Nope</b>}
                </ul>                
        </div>
    )
}

export default GuestsEdit;