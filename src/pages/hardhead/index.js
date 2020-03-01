import React, { Component } from 'react';
import config from 'react-global-configuration';
import { PostSmallImage } from '../../components';
import Guests from './guests';
import Rating from './rating.js';
import Movie from './movie.js';

export default class Hardhead extends Component {
	constructor(props) {
		super(props);
		this.state = {
			isLoaded : false,
			error: null,
			hardheads: null
		};
	}

	componentDidMount() {
		this.getHardheadData();
	}

	getHardheadData() {
		var url = config.get('path') + '/api/hardhead?dateFrom=2019-01-01&code=' + config.get('code');		
    
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
					{[].concat(this.state.hardheads)
					.map((hardhead, i) => 
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
								</span>
							}
							right={ <Guests hardheadID={hardhead.ID} /> }
							bottom={ <Movie id={hardhead.ID}/> }
							stats={<Rating />}	
						/>											
					)}
				</div>
			)
		}
    }
}