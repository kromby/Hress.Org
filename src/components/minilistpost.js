import React, { Component } from 'react';
import UserLink from './users/userlink';
import './post.css';

export default class MiniListPost extends Component {
    render() {
        return (
            <article className="mini-post">
                <header>
                    <h3><a href="single.html">{this.props.title}</a></h3>
                    <span className="published">{this.props.description}</span>
                </header>
                <div>
                    <ol>
                        {this.props.list.map(entity => (
                            <li key={entity.ID}>
                                {entity.UserID ? <UserLink UserId={entity.UserID}/> : null}
                                {entity.UserID ? " - " : null}
                                {entity.Text}
                            </li>
                        ))}                               
                    </ol>
                </div>
            </article>
        )
    }
}