import React, { useEffect, useState } from 'react';
import config from 'react-global-configuration';
import { Post } from '../../components';
import HardheadRating from './components/rating.js';
import * as qs from 'query-string';
import HardheadActions from './components/actions';
// import VoteNow from './awards/election/votenow';
import axios from "axios";
import HardheadBody from './components/hardheadbody';
import VoteNow from './awards/election/votenow';

const Hardhead = (propsData) => {
	const [hardheads, setHardheads] = useState();	
	const [lastUrl , setLastUrl] = useState();

	useEffect(() => {
		const getHardheads = async (url) => {
			try {
				const response = await axios.get(url);
				setHardheads(response.data);
			}
			catch (e) {
				console.error(e);
			}
		}

		const getHardheadsUrl = () => {
			const parsed = qs.parse(propsData.location.search);
			console.log("getHardheadsUrl");
			console.log(parsed);
			var url;
			if (parsed.parentID) {
				url = config.get('path') + '/api/hardhead?parentID=' + parsed.parentID + '&code=' + config.get('code');
			} else if (parsed.userID) {
				url = config.get('path') + '/api/hardhead?userID=' + parsed.userID + '&code=' + config.get('code');
			} else if(parsed.query) {
				url = config.get('path') + '/api/movies?filter=' + parsed.query + '&code=' + config.get('code');
			}
			else {
				var currentDate = new Date();
				currentDate.setMonth(currentDate.getMonth() - 5);
				url = config.get('path') + '/api/hardhead?dateFrom=' + (currentDate.getMonth() + 1) + '.1.' + currentDate.getFullYear() + '&code=' + config.get('code');
			}
			return url;
		}

		document.title = "Harðhaus | Hress.Org";

		var url = getHardheadsUrl();

		if(!hardheads || lastUrl != url) {
			getHardheads(url);
			setLastUrl(url);
		}
	}, [propsData])

	return (
		<div id="main">
			<VoteNow />

			{ hardheads ?
				hardheads.map((hardhead) =>
				<Post
					key={hardhead.ID}
					id={hardhead.ID}
					title={hardhead.Name}
					description={hardhead.GuestCount ? hardhead.GuestCount + " gestir" : null}
					date={hardhead.Date}
					dateFormatted={hardhead.DateString}
					author={hardhead.Host}
					body={ <HardheadBody id={hardhead.ID} name={hardhead.Name} description={hardhead.Description} />}
					actions={<HardheadActions id={hardhead.ID} />}
					stats={<HardheadRating id={hardhead.ID} />}
				/>
			) : null}
		</div>
	)
}

export default Hardhead;