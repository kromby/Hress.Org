import React, { Component } from 'react';
import config from 'react-global-configuration';
import Author from '../../../components/author';

export default class Guests extends Component {
	constructor(props) {
		super(props);
		this.state = {
			isLoaded : false,
            error: null,
            guests: null
		};
	}

	componentDidMount() {
		this.getData(this.props.hardheadID);
    }

    getData(id)
    {
        var url = config.get('path') + '/api/hardhead/' + id + '/guests?code=' + config.get('code');
    
        fetch(url, {
            method: 'GET' 
        })
        .then(res => res.json())
        .then((result) => {
            this.setState({error: null, isLoaded: true, guests: result});
        }, 
        (error) => {
            this.setState({isLoaded: true, error: error});
        });  
    }

    render()
    {
        const { error, isLoaded, guests } = this.state;

		if (error) {
			return (
				<div id="main">
                    {/* {error} */}
                </div>
			)
		} else if (!isLoaded) {
            return (
				<div id="main">Loading</div>
			)
		} else {
			return (
                <div className="row gtr-uniform">
                        <div className="col-12">
                            <h3>Gestir</h3>
                        </div>
                        {guests.map(guest =>                            
                            <div className="col-4" key={guest.ID}>
                                {/* <Author href={guest.Href}/> */}
                                {typeof guest.ProfilePhoto !=='undefined' ?
                                <Author ID={guest.ID} Username={guest.Username} ProfilePhoto={guest.ProfilePhoto.Href} /> :
                                <Author ID={guest.ID} Username={guest.Username} />}
                            </div>                    
                        )}
                </div>
            )
        }
    }
}