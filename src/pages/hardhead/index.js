import React, { Component } from 'react';
import config from 'react-global-configuration';
import { PostSmallImage } from '../../components';
import Guests from './components/guests';
import HardheadRating from './components/rating.js';
import Movie from './components/movie.js';
import * as qs from 'query-string';

export default class Hardhead extends Component {

	constructor(props) {
		super(props);
		this.state = {
			isLoaded : false,
			error: null,
			hardheads: null,
			parent: 0
		};
	}

	componentDidMount() {
		this.getHardheadData();
	}

	componentDidUpdate() {
		const parsed = qs.parse(this.props.location.search);
		if(this.state.parent !== parsed.parentID)
			this.getHardheadData();
	}

	getHardheadData() {
		const parsed = qs.parse(this.props.location.search);
		console.log('parsed: ' & parsed.parentID);
		this.setState({parent: parsed.parentID});

		var currentDate = new Date();
		currentDate.setMonth(currentDate.getMonth() - 5);
		var url;
		if(parsed.parentID) {
			url = config.get('path') + '/api/hardhead?parentID=' + parsed.parentID + '&code=' + config.get('code');		
		} else {
			url = config.get('path') + '/api/hardhead?dateFrom=' + (currentDate.getMonth()+1) + '.1.' + currentDate.getFullYear() + '&code=' + config.get('code');		
		}
    
        fetch(url, {
            method: 'GET' 
        })
        .then(res => res.json())
        .then((result) => {			
			this.setState({error: null, isLoaded: true, hardheads: result});		
        },
        (error) => {
			console.log(error);
            this.setState({isLoaded: true, error});
        });  
	}
	
    render() {
		const { error, isLoaded } = this.state;

		if (error) {
			return (
				<div id="main">{error}</div>
			)
		} else if (!isLoaded) {
            return (
				<div id="main">Loading</div>
			)
		} else {
			return (
				<div id="main">
					{[].concat(this.state.hardheads).map((hardhead, i) => 
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
									{hardhead.Description ? hardhead.Description : "Líklega hefur ekkert merkilegt gerst fyrst gestgjafi hefur ekki skráð neitt."}
									<br/>
									<br/>
									<Guests hardheadID={hardhead.ID} />
								</span>
							}
							right={ <Movie id={hardhead.ID}/> }
							// bottom={  }
							stats={<HardheadRating id={hardhead.ID} />}	
						/>											
					)}
				</div>
			)
		}
    }
}