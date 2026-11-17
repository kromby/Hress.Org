import { Component } from 'react';
import { Link } from 'react-router-dom';

import Author from './author';
import './post.css';

export default class Post extends Component {
    render() {
        return (
            <article className="post">
                <header>
                    {this.props.thumbnail ? (
                        // A small image beside the title (on phones, above it).
                        <div className="title has-thumbnail">
                            {this.props.href ?
                                <Link to={this.props.href} className="thumbnail" tabIndex={-1} aria-hidden="true">{this.props.thumbnail}</Link>
                                : <div className="thumbnail">{this.props.thumbnail}</div>}
                            <div>
                                <h2>{this.props.href ? <Link to={this.props.href}>{this.props.title}</Link> : this.props.title}</h2>
                                {this.props.description ? <p>{this.props.description}</p> : null}
                            </div>
                        </div>
                    ) : (
                        <div className="title">
                            <h2>{this.props.href ? <Link to={this.props.href}>{this.props.title}</Link> : this.props.title}</h2>
                            {this.props.description ? <p>{this.props.description}</p> : null}
                        </div>
                    )}
                    {/* <div className="meta">
                        {this.props.date ?
                            <time className="published" dateTime={this.props.date}>{this.props.dateFormatted}</time>
                            : null}
                        {this.props.author ?
                            <Author href={this.props.author} />
                            : null}
                    </div> */}
                    <div className="meta">
                        {this.props.meta ?? (
                          <>
                            <time className="published" dateTime={this.props.date}>{this.props.dateFormatted}</time>
                            {(this.props.author?.id || this.props.author?.ID) && (
                              <Author
                                id={this.props.author.id || this.props.author.ID}
                                username={this.props.author.username || this.props.author.Username}
                                userPath={this.props.userPath}
                                profilePhoto={this.props.author.profilePhoto?.href || this.props.author.ProfilePhoto?.Href}
                              />
                            )}
                          </>
                        )}
                    </div>
                </header>
                {this.props.image ?
                    <figure className="image featured">
                        {this.props.href ?
                            <Link to={this.props.href}><img src={this.props.image} alt={this.props.title} /></Link>
                            : <img src={this.props.image} alt={this.props.title} />}
                    </figure>
                    : null}
                {/* A post with only a header (e.g. an item in a list) renders nothing below it. */}
                {this.props.body ? <section>{this.props.body}</section> : (this.props.actions || this.props.stats) ? <p /> : null}
                {(this.props.actions || this.props.stats) && (
                    <footer>
                        {this.props.actions}
                        {this.props.stats}
                    </footer>
                )}
            </article>
        )
    }
}