import { useEffect, useState } from 'react';
import config from 'react-global-configuration';
import { Post } from '../../components';
import DatePicker from 'react-datepicker';
import "react-datepicker/dist/react-datepicker.css"
import axios from "axios";
import { useAuth } from '../../context/auth';
import MovieEdit from './components/movieEdit';
import GuestsEdit from './components/guestsEdit';
import { useLocation, useNavigate, useParams } from 'react-router-dom';

const HardheadEdit = () => {
	const { authTokens } = useAuth();
	const location = useLocation();
	const params = useParams();
	const navigate = useNavigate();
	const [hardhead, setHardhead] = useState();
	const [users, setUsers] = useState();
	const [nextHostID, setNextHostID] = useState();
	const [hardheadDate, setDate] = useState(new Date());
	const [description, setDescription] = useState("");
	const [data, setData] = useState({ isLoaded: false, hardhead: null, description: null, hardheadDate: new Date(), saved: false });
	const [buttonEnabled, setButtonEnabled] = useState(false);

	useEffect(() => {
		if (authTokens === undefined) {
			navigate("/login", { state: { from: location.pathname } });
			return;
		}

		const getHardhead = async () => {
			try {
				const url = config.get('apiPath') + '/api/hardhead/' + params.hardheadID;
				const response = await axios.get(url)
				const date = new Date(response.data.date);
				setHardhead(response.data);
				setDate(date);
				setDescription(response.data.description)
				setData({ error: null, isLoaded: true, visible: true, minDate: new Date(date.getFullYear(), date.getMonth(), 1), maxDate: new Date(date.getFullYear(), date.getMonth() + 1, 0) });
			} catch (e) {
				console.error(e);
				setData({ isLoading: false, visible: false });
			}
		}

		const getUsers = async () => {
			try {
				const url = config.get('path') + '/api/users?role=US_L_HEAD&code=' + config.get('code')
				const response = await axios.get(url);
				setUsers(response.data);
			} catch (e) {
				console.error(e);
			}
		}

		if (!hardhead) {
			getHardhead();
		}
		if (!users) {
			getUsers();
		}
	}, [params, location, authTokens])

	const handleSubmit = async (event) => {
		setButtonEnabled(false);
		event.preventDefault();
		try {
			const url = config.get('apiPath') + '/api/hardhead/' + params.hardheadID;
			await axios.put(url, {
				ID: params.hardheadID,
				Number: hardhead.number,
				Host: hardhead.host,
				Date: hardheadDate,
				Description: description,
				NextHostID: nextHostID
			}, {
				headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
			});
			setData({ saved: true, error: data.error, isLoaded: data.isLoaded, visible: data.visible, minDate: data.minDate, maxDate: data.maxDate });
			
		} catch (e) {
			console.error(e);
			alert("Ekki tókst að vista kvöld.");
			setButtonEnabled(true);
		}
	}

	const handleDescriptionChange = (event) => { setDescription(event.target.value); setButtonEnabled(true); }
	const handleDateChange = (event) => { setDate(event); setButtonEnabled(true); }
	const handleHostChange = (event) => { setNextHostID(event.target.value); setButtonEnabled(true); }

	return (
		<div id="main">
			{data.visible ?
				<Post
					key={hardhead.id}
					id={hardhead.id}
					title={hardhead.name}
					description={hardhead.guestCount + " gestir"}
					date={hardhead.date}
					dateFormatted={hardhead.dateString}
					author={hardhead.host}
					userPath="/hardhead/users/"
					body={[
						<section key="edit1">
							<h3>Kvöldið</h3>
							<form onSubmit={handleSubmit}>
								<div className="row gtr-uniform">
									<div className="col-6 col-12-xsmall">
										<DatePicker selected={hardheadDate} onChange={(date) => handleDateChange(date)} dateFormat="dd.MM.yyyy" minDate={data.minDate} maxDate={data.maxDate} />
									</div>
									<div className="col-6 col-12-xsmall">
										{users ?
											<select id="demo-category" name="demo-category" onChange={(ev) => handleHostChange(ev)}>
												<option value="">- Á hvern skoraðir þú? -</option>
												{users.sort((a, b) => a.Name > b.Name ? 1 : -1).map(user =>
													<option key={user.ID} value={user.ID}>
														{user.Name}
													</option>
												)}
											</select>
											: null}
									</div>
									<div className="col-12">
										<textarea name="Lýsing" rows="3" onChange={(ev) => handleDescriptionChange(ev)} defaultValue={hardhead.description} placeholder="Lýstu kvöldinu" />
									</div>
									<div className="col-12">
										{data.saved ? <b>Kvöld vistað!<br /></b> : null}
										<button tooltip="Vista kvöld" className="button large" disabled={!buttonEnabled}>Vista kvöld</button>
									</div>
								</div>
							</form>
						</section>,
						<MovieEdit key="edit2" id={hardhead.id} />,
						<GuestsEdit key="edit3" hardheadID={hardhead.id} users={users} />
					]}
				/>
				: null}
		</div>
	)
}

export default HardheadEdit;