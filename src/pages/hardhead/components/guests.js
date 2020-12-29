import React, { Component } from 'react';
import config from 'react-global-configuration';
import UserImage from '../../../components/users/userimage';

export default class Guests extends Component {
    constructor(props) {
        super(props);
        this.state = {
            isLoaded: false,
            error: null,
            guests: null
        };
    }

    componentDidMount() {
        this.getData(this.props.hardheadID);
    }

    getData(id) {
        var url = config.get('path') + '/api/hardhead/' + id + '/guests?code=' + config.get('code');

        fetch(url, {
            method: 'GET'
        })
            .then(res => res.json())
            .then((result) => {
                this.setState({ error: null, isLoaded: true, guests: result });
            },
                (error) => {
                    this.setState({ isLoaded: true, error: error });
                });
    }

    render() {
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
                <section>
                    <h3>Gestir</h3>
                    <div className="row gtr-uniform">
                        {guests.map(guest =>
                            <div className="col-2 align-center" key={guest.ID}>
                                {typeof guest.ProfilePhoto !== 'undefined' ?
                                    <UserImage id={guest.ID} username={guest.Username} profilePhoto={guest.ProfilePhoto.Href} /> :
                                    <UserImage id={guest.ID} username={guest.Username} />
                                }
                            </div>
                        )}
                    </div>
                </section>
            )
        }
    }
}