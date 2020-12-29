// import React, { Component } from 'react';
// import config from 'react-global-configuration';
// import { Post, PostSmallImage } from '../../components';
// import Guests from './components/guests';
// import HardheadRating from './components/rating.js';
// import Movie from './components/movie.js';
// import * as qs from 'query-string';
// import HardheadActions from './components/actions';
// import VoteNow from './awards/election/votenow';

// export default class Hardhead extends Component {

// 	constructor(props) {
// 		super(props);
// 		this.state = {
// 			isLoaded: false,
// 			error: null,
// 			hardheads: null,
// 			url: null,
// 			moviePhoto: null
// 		};
// 	}

// 	componentDidMount() {
// 		var url = this.getHardheadUrl();

// 		if (this.state.url !== url) {
// 			console.log("componentDidMount");
// 			this.setState({ url: url });
// 			this.getHardheadData(url);
// 		}
// 	}

// 	componentDidUpdate() {
// 		var url = this.getHardheadUrl();

// 		if (this.state.url !== url) {
// 			console.log("componentDidUpdate");
// 			this.setState({ url: url });
// 			this.getHardheadData(url);
// 		}
// 	}

// 	getHardheadUrl() {
// 		const parsed = qs.parse(this.props.location.search);
// 		var url;
// 		if (parsed.parentID) {
// 			url = config.get('path') + '/api/hardhead?parentID=' + parsed.parentID + '&code=' + config.get('code');
// 		} else if (parsed.userID) {
// 			url = config.get('path') + '/api/hardhead?userID=' + parsed.userID + '&code=' + config.get('code');
// 		} else {
// 			var currentDate = new Date();
// 			currentDate.setMonth(currentDate.getMonth() - 5);
// 			url = config.get('path') + '/api/hardhead?dateFrom=' + (currentDate.getMonth() + 1) + '.1.' + currentDate.getFullYear() + '&code=' + config.get('code');
// 		}
// 		return url;
// 	}

// 	getHardheadData(url) {
// 		fetch(url, {
// 			method: 'GET'
// 		})
// 			.then(res => res.json())
// 			.then((result) => {
// 				this.setState({ error: null, isLoaded: true, hardheads: result });

// 				if (this.props.match.params.hardheadID) {
// 					this.setState({ description: result.Description });
// 				}
// 			},
// 				(error) => {
// 					console.log(error);
// 					this.setState({ isLoaded: true, error });
// 				});
// 	}

// 	photoPostback(src) {
// 		console.log("photoPostback");
// 		console.log(src);
// 		if (src) {
// 			this.setState({ moviePhoto: src });
// 		}
// 	}

// 	render() {
// 		const { error, isLoaded } = this.state;

// 		if (error) {
// 			return (
// 				<div id="main">{error}</div>
// 			)
// 		} else if (!isLoaded) {
// 			return (
// 				<div id="main">Loading</div>
// 			)
// 		} else {
// 			return (
// 				<div id="main">
// 					<VoteNow />

// 					{[].concat(this.state.hardheads).map((hardhead, i) =>
// 						[
// 							<Post
// 								key={hardhead.ID}
// 								id={hardhead.ID}
// 								title={hardhead.Name}
// 								description={hardhead.GuestCount + " gestir"}
// 								date={hardhead.Date}
// 								dateFormatted={hardhead.DateString}
// 								author={hardhead.Host}
// 								body={[
// 									this.state.moviePhoto ?
// 										<span className="image right"><img src={this.state.moviePhoto} alt={hardhead.Name} /></span> : null,
// 									<section key="0">
// 										<h3>Kvöldið</h3>
// 										<p>
// 											{hardhead.Description ? hardhead.Description : "Líklega hefur ekkert merkilegt gerst fyrst gestgjafi hefur ekki skráð neitt."}
// 										</p>
// 									</section>,
// 									<Movie key="1" id={hardhead.ID} photoPostback={this.photoPostback} />,
// 									<Guests key="2" hardheadID={hardhead.ID} />,
// 									<p key="3"></p>,
// 								]}
// 								actions={<HardheadActions id={hardhead.ID} />}
// 								stats={<HardheadRating id={hardhead.ID} />}
// 							/>,
// 							// <PostSmallImage
// 							// 	key={hardhead.ID} 
// 							// 	id={hardhead.ID} 
// 							// 	title={hardhead.Name}
// 							// 	description={hardhead.GuestCount + " gestir"}
// 							// 	date={hardhead.Date}
// 							// 	dateFormatted={hardhead.DateString}
// 							// 	author={hardhead.Host}
// 							// 	left={
// 							// 		<section>
// 							// 			<h3>Kvöldið</h3>
// 							// 			<p>
// 							// 				{hardhead.Description ? hardhead.Description : "Líklega hefur ekkert merkilegt gerst fyrst gestgjafi hefur ekki skráð neitt."}
// 							// 			</p>
// 							// 			<Guests hardheadID={hardhead.ID} />
// 							// 		</section>
// 							// 	}
// 							// 	right={ <Movie id={hardhead.ID}/> }
// 							// 	actions={ <HardheadActions id={hardhead.ID} /> }
// 							// 	// bottom={  }
// 							// 	stats={<HardheadRating id={hardhead.ID} />}	
// 							// />
// 						]
// 					)}
// 				</div>
// 			)
// 		}
// 	}
// }