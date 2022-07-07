import React from 'react';
import { Link } from 'react-router-dom';
import './intro.css';

const Intro = (propsData) => {
    return (
        <section id="intro">
            <Link to="/" className="logo">
                <img src={propsData.logo} alt="" />
            </Link>
            <header>
                <h2>{propsData.title}</h2>
                <p>{propsData.description}</p>
            </header>
        </section>
    )
}

export default Intro;