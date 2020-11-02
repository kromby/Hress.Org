import React, { Component, useEffect, useState } from 'react';
import config from 'react-global-configuration';
import { PostSmallImage } from '../../components';
import Guests from './components/guests';
import Movie from './components/movie.js';
import DatePicker from 'react-datepicker';
import "react-datepicker/dist/react-datepicker.css"
import axios from "axios";
import { useAuth } from '../../context/auth';

const HardheadEdit = (propsData) => {
	const {authTokens} = useAuth();
	const [hardhead, setHardhead] = useState();
	const [users, setUsers] = useState();
	const [nextHostID, setNextHostID] = useState();
	const [hardheadDate, setDate] = useState(new Date());
	const [description, setDescription] = useState("");
	const [data, setData] = useState({isLoaded: false, hardhead: null, description: null, hardheadDate: new Date(), saved: false});
		
		// this.handleSubmit = this.handleSubmit.bind(this);
		// this.handleDescriptionChange = this.handleDescriptionChange.bind(this);
		// this.handleDateChange = this.handleDateChange.bind(this);

	useEffect(() => {
		if(authTokens == undefined) {
			// TODO Redirect back to main page
		}

		const getHardhead = async () => {
			try {
				var url = config.get('path') + '/api/hardhead/' + propsData.match.params.hardheadID + '?code=' + config.get('code');
				const response = await axios.get(url)
				var date = new Date(response.data.Date);
				setHardhead(response.data);
				setDate(date);
				setDescription(response.data.Description)
				setData({error: null, isLoaded: true, visible: true, minDate: new Date(date.getFullYear(), date.getMonth(), 1), maxDate: new Date(date.getFullYear(), date.getMonth()+1, 0)});
			} catch(e) {
				console.error(e);
				setData({isLoading: false, visible: false});
			}
		}

		const getUsers = async () => {
			try {
				var url = config.get('path') + '/api/users?role2=hardhead&code=' + config.get('code')
				const response = await axios.get(url);
				setUsers(response.data);
			} catch (e) {
				console.error(e);				
			}
		}

		getHardhead();
		getUsers();
	}, [propsData, authTokens])

	const handleSubmit = async (event) => {
		if(authTokens !== undefined){			
			event.preventDefault();			
			try {
				var url = config.get('path') + '/api/hardhead/' + propsData.match.params.hardheadID + '?code=' + config.get('code');
				const response = await axios.put(url, {
					ID: propsData.match.params.hardheadID,
					Number: hardhead.Number,
					Host: hardhead.Host,
					Date: hardheadDate,
					Description: description,
					NextHostID: nextHostID			
				}, {
					headers: {'Authorization': 'token ' + authTokens.token},            
				});
				setData({saved: true, error: data.error, isLoaded: data.isLoaded, visible: data.visible, minDate: data.minDate, maxDate: data.maxDate});
				console.log(response);
			} catch(e) {
				console.error(e);
				alert("Ekki tókst að vista kvöld.");
			}
		} else {
			// TODO: redirect to main page
		}
	}	

	const handleDescriptionChange = (event) => { setDescription(event.target.value);  }
	const handleDateChange = (event) => { setDate(event); }
	const handleHostChange = (event) => { setNextHostID(event.target.value); }
	
	return (
		<div id="main">
			{data.visible ? 
				<form onSubmit={handleSubmit}>
					<PostSmallImage
						key={hardhead.ID} 
						id={hardhead.ID} 
						title={hardhead.Name}
						description={hardhead.GuestCount + " gestir"}
						date={hardhead.Date}
						dateFormatted={hardhead.DateString}
						author={hardhead.Host}
						left={
							<span>
								<h3>Kvöldið</h3>
								<DatePicker selected={hardheadDate} onChange={(date) => handleDateChange(date)} dateFormat="dd.MM.yyyy" minDate={data.minDate} maxDate={data.maxDate} />
								<br/>
								<br/>
								<textarea name="Lýsing" rows="3" onChange={(ev) => handleDescriptionChange(ev)} defaultValue={hardhead.Description} placeholder="Lýstu kvöldinu" />
								<br/>
								<br/>								
								{/* <Guests hardheadID={hardhead.ID} /> */}
								{users ? 
									<select id="demo-category" name="demo-category" onChange={(ev) => handleHostChange(ev)}>
										<option value="">- Á hvern skoraðir þú? -</option>
									{users.sort((a,b) => a.Name > b.Name ? 1 : -1).map(user => 
										<option key={user.ID} value={user.ID}>
											{user.Name}
										</option>
									)}
									</select>
								: null}
								{data.saved ? <b>Kvöld vistað!</b> : null}
								<br/>
								<br/>
							</span>
						}
						// right={ <Movie id={hardhead.ID}/> }
						actions={
							<ul className="actions">
								<li>
									<button tooltip="Vista" className="button large">Vista</button>
								</li>							
							</ul>
						}
						// bottom={  }
						// stats={<HardheadRating id={hardhead.ID} />}	
					/>	
				</form> : null}
		</div>
	)
}

export default HardheadEdit;