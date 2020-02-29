import React, { Component } from 'react';
import './post.css';

export default class MiniPost extends Component {
    render() {
        return (
            <article className="mini-post">
                <header>
                    <h3><a href="single.html">{this.props.title}</a></h3>
                    <time className="published" datetime="2015-10-19">{this.props.date}</time>
                    {/* <a href="#" className="author"><img src="images/avatar.jpg" alt="" /></a> */}
                </header>
                <a href="single.html" className="image"><img src="images/pic05.jpg" alt="" /></a>
            </article>
        )
    }
}