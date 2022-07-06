import React, { Component } from 'react';
import { Link } from 'react-router-dom';

import Author from './author';
import './post.css';

export default class Post extends Component {
    render() {
        return (
            <article className="post">
                <header>
                    <div className="title">
                        <h2>{this.props.href ? <Link to={this.props.href}>{this.props.title}</Link> : this.props.title}</h2>
                        {this.props.description ? <p>{this.props.description}</p> : null}
                    </div>
                    {/* <div className="meta">
                        {this.props.date ?
                            <time className="published" dateTime={this.props.date}>{this.props.dateFormatted}</time>
                            : null}
                        {this.props.author ?
                            <Author href={this.props.author} />
                            : null}
                    </div> */}
                    <div className="meta">
                        <time className="published" dateTime={this.props.date}>{this.props.dateFormatted}</time>
                        {this.props.author && this.props.author.ID ?
                            typeof this.props.author.ProfilePhoto !== 'undefined' ?
                                <Author ID={this.props.author.ID} Username={this.props.author.Username} ProfilePhoto={this.props.author.ProfilePhoto.Href} /> :
                                <Author ID={this.props.author.ID} Username={this.props.author.Username} />
                            : null}
                        {this.props.author && this.props.author.id ?
                            typeof this.props.author.profilePhoto !== 'undefined' ?
                                <Author ID={this.props.author.id} Username={this.props.author.username} ProfilePhoto={this.props.author.profilePhoto.href} /> :
                                <Author ID={this.props.author.id} Username={this.props.author.username} />
                            : null}
                    </div>
                </header>
                {this.props.image ?
                    <a href={"?id=" + this.props.id} class="image featured"><img src={this.props.image} alt="" /></a>
                    : null}
                {this.props.body ? <section>{this.props.body}</section> : <p />}
                <footer>
                    {this.props.actions}
                    {this.props.stats}
                </footer>
            </article>
        )
    }
}