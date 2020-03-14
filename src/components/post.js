import React, { Component } from 'react';
import Author from './author';
import './post.css';

export default class Post extends Component {
    render() {
        return (
            <article className="post">
                <header>
                    <div className="title">
                        <h2><a href={"?id=" + this.props.id}>{this.props.title}</a></h2>
                        {this.props.description ? <p>{this.props.description}</p> : null}
                    </div>
                    <div className="meta">
                        {this.props.date ? 
                        <time className="published" dateTime={this.props.date}>{this.props.dateFormatted}</time>
                        : null }
                        {this.props.author ? 
                        <Author href={this.props.author}/>
                        : null }
                    </div>
                </header>
                {this.props.image ?
                <a href={"?id=" + this.props.id} class="image featured"><img src={this.props.image} alt="" /></a>
                : null }
                {this.props.body ? <div className="published">{this.props.body}</div> : <p/> }
                {/* <footer>
                    <ul class="actions">
                        <li><a href={"?id=" + this.props.id} class="button large">Continue Reading</a></li>
                    </ul>
                    <ul class="stats">
                        <li><a href="#">General</a></li>
                        <li><a href="#" class="icon solid fa-heart">28</a></li>
                        <li><a href="#" class="icon solid fa-comment">128</a></li>
                    </ul>
                </footer> */}
            </article>
        )
    }
}