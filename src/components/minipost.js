import React from 'react';
import './post.css';

const MiniPost = (propsData) => {
    return (
        <article className="mini-post">
            <header>
                <h3>
                    {propsData.href ? <a href={propsData.href}>{propsData.title}</a> : propsData.title}
                </h3>
                {propsData.description ? <span className="published">{propsData.description}</span> : null }
                <time className="published" datetime={propsData.date}>{propsData.dateString}</time>
                <a href={propsData.userHref} className="author"><img src={propsData.userPhoto} alt={propsData.userText} /></a>
            </header>
            {propsData.imageSource ?
                propsData.imageHref ?            
                    <a href={propsData.imageSource} className="image"><img src={propsData.imageSource} alt={propsData.imageText} /></a> :
                    <img src={propsData.imageSource} alt={propsData.imageText} /> :
            null
            }
        </article>
    )
}

export default MiniPost;