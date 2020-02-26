import React from 'react';
import config from 'react-global-configuration';

class Author extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            isLoaded: false,
            error: null,
            user: null
        }
    }

    componentDidMount() {
        if(typeof this.props.href !== 'undefined') {
            var url = config.get('path') + this.props.href + '?code=' + config.get('code');
        
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
        } else {
            this.setState({error: null, isLoaded: true, user: {
                ID: this.props.ID,
                Username: this.props.Username,
                ProfilePhoto: {
                    Href: this.props.ProfilePhoto
                }
            }});
        }
    }

    render() {
        const { error, isLoaded, user } = this.state;

        if (error) {
            return <span className="name">{error.message}</span>;
        } else if (!isLoaded) {
            return <a href={this.props.href} className="author"><span className="name">{this.props.href}</span><img src="images/avatar.jpg" alt="" /></a>;
        } else {
            return (
                <a href={"http://www.hress.org/Gang/Single.aspx?Id=" + user.ID} className="author">
                    <span className="name">{user.Username}</span>
                    {typeof user.ProfilePhoto !=='undefined' ?
                    <img src={"https://ezhressapi.azurewebsites.net" + user.ProfilePhoto.Href + "?code=cCzROHQTlutKpUWOy/BexCj7YkDyFLhGyk4oAEot3eHo1wBgoX/dXQ=="} alt={user.Username} /> :
                    null}
                </a>
            );
        }
    }
}

export default Author;