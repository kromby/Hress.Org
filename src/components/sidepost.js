import React from 'react';
import { Link } from 'react-router-dom';

const SidePost = (propsData) => {
    return (
        <article>
            <header>
                <h3>
                    <Link to={propsData.href}>{propsData.title}</Link>
                    {/* <a href={propsData.href}>{propsData.title}</a> */}
                </h3>
                {propsData.dateString ?
                <time className="published" dateTime={propsData.date}>{propsData.dateString}</time>
                : null}
            </header>
            {propsData.image ?
            <Link to={propsData.href} className="image"><img src={propsData.image} alt={propsData.imageText} /></Link>
            // <a href={propsData.href} className="image"><img src={propsData.image} alt={propsData.imageText} /></a>
            : null}
        </article>
    )
}

export default SidePost;