import React from 'react';
import './post.css';
import { Link } from 'react-router-dom';

interface MiniPostProps {
  title: string;
  description: React.ReactNode;
  date?: Date | string;
  dateString?: string;
  href?: string;
  imageText?: string;
  imageSource?: string;
  imageHref?: string;
  userText?: string;
  userPhoto?: string;
  userHref?: string;
}

const MiniPost = ({title, description, date, dateString = '', href = undefined, imageText = '', imageSource = undefined, imageHref = undefined, userText = '', userPhoto = '', userHref = ''}: MiniPostProps) => {
    return (
        <article className="mini-post">
            <header>
                <h3>                    
                    {href ? 
                        href.startsWith("http") ?
                        <a href={href} target="_parent">{title}</a>
                        : 
                        <Link to={href}>{title}</Link>
                    : title }
                </h3>
                {description ? <span className="published">{description}</span> : null }
                <time className="published" dateTime={date instanceof Date ? date.toISOString() : date || ''}>{dateString}</time>
                <a href={userHref} className="author"><img src={userPhoto} alt={userText} /></a>
            </header>
            {imageSource ?
                imageHref ?            
                    <Link to={imageHref} className="image"><img src={imageSource} alt={imageText} /></Link> :
                    <img src={imageSource} alt={imageText} /> :
            null
            }
        </article>
    )
}

export default MiniPost;