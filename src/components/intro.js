import React, { Component } from 'react';

export default class Intro extends Component {
    render() {
        return (
            <section id="intro">
                <a href="http://www.hress.org" className="logo">
                    <img src={this.props.logo} alt="" />
                </a>
                <header>
                    <h2>{this.props.title}</h2>
                    <p>{this.props.description}</p>
                </header>
            </section>
        )
    }
}