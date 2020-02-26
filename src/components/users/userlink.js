import React from 'react';
import config from 'react-global-configuration';

class UserLink extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            isLoaded: false,
            error: null,
            user: null
        }
    }

    componentDidMount() {
        var url = config.get('path') + '/api/users/' + this.props.UserId + '?code=' + config.get('code');
    
        fetch(url, {
            method: 'GET' 
        })
        .then(res => res.json())
        .then((result) => {
            this.setState({error: null, isLoaded: true, user: result});
        }, 
        (error) => {
            this.setState({isLoaded: true, error});
        });  
    }

    render() {
        const { error, isLoaded, user } = this.state;

        if (error) {
            return <div>{error.message}</div>;
        } else if (!isLoaded) {
            return <a href={'http://www.hress.org/Gang/Single.aspx?id=' + this.props.UserId}>{this.props.UserId}</a>;
        } else {
            return <a href={'http://www.hress.org/Gang/Single.aspx?id=' + this.props.UserId}>{user.Username}</a>;
        }
    }
}

export default UserLink;