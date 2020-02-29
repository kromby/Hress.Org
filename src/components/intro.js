import React from 'react';
import './intro.css';

const Intro = (propsData) => {
    return (
        <section id="intro">
            <a href="http://www.hress.org" className="logo">
                <img src={propsData.logo} alt="" />
            </a>
            <header>
                <h2>{propsData.title}</h2>
                <p>{propsData.description}</p>
            </header>
        </section>
    )
}

export default Intro;