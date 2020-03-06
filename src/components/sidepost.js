import React from 'react';

const SidePost = (propsData) => {
    return (
        <article>
            <header>
                <h3><a href={propsData.href}>{propsData.title}</a></h3>
                {propsData.dateString ?
                <time class="published" datetime={propsData.date}>{propsData.dateString}</time>
                : null}
            </header>
            {propsData.image ?
            <a href={propsData.href} class="image"><img src={propsData.image} alt={propsData.imageText} /></a>
            : null}
        </article>
    )
}

export default SidePost;