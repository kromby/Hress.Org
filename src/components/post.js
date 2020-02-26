import React, { Component } from 'react';
import Author from './author';

export default class Post extends Component {
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
                        <Author href={this.props.author}/>
                    </div>
                </header>
                <a href={"?id=" + this.props.id} class="image featured"><img src={this.props.image} alt="" /></a>
                {/* <p>Mauris neque quam, fermentum ut nisl vitae, convallis maximus nisl. Sed mattis nunc id lorem euismod placerat. Vivamus porttitor magna enim, ac accumsan tortor cursus at. Phasellus sed ultricies mi non congue ullam corper. Praesent tincidunt sed tellus ut rutrum. Sed vitae justo condimentum, porta lectus vitae, ultricies congue gravida diam non fringilla.</p> */}
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