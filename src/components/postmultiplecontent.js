import React, { Component } from 'react';
import Author from './author';

export default class PostMultipleContent extends Component {
    render() {
        return (
            <article className="post">
                <header>
                    <div className="title">
                        <h2><a href={"?id=" + this.props.id}>{this.props.title}</a></h2>
                        <p>{this.props.description}</p>
                    </div>
                    <div className="meta">
                        <time className="published" dateTime={this.props.date}>{this.props.dateFormatted}</time>
                        {typeof this.props.author.ProfilePhoto !=='undefined' ?
                        <Author ID={this.props.author.ID} Username={this.props.author.Username} ProfilePhoto={this.props.author.ProfilePhoto.Href} /> :
                        <Author ID={this.props.author.ID} Username={this.props.author.Username} />}
                    </div>
                </header>
                {this.props.top ? <div className="image featured">{this.props.top}</div> : null}
                <div className="row">
                    {this.props.left ? <div className="col-6 col-12-medium">{this.props.left}</div> : null}
                    {this.props.right ? <div className="col-6 col-12-medium">{this.props.right}</div> : null}
                </div>
                <div className="row">
                    {this.props.bottom}
                </div>
                <footer>
                    <ul className="actions">
                        {/* <li><a href={"?id=" + this.props.id} className="button large">Continue Reading</a></li>                         */}
                    </ul>
                    <ul className="stats">
                        {/* <li><a href="#">General</a></li>
                        <li><a href="#" className="icon solid fa-heart">28</a></li>
                        <li><a href="#" className="icon solid fa-comment">128</a></li> */}
                        {this.props.stats}
                    </ul>
                </footer>
            </article>
        )
    }
}