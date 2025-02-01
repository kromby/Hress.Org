import './post.css';
import { Link } from 'react-router-dom';

const MiniPost = ({title, description, date, dateString = '', href = undefined, imageText = '', imageSource = undefined, imageHref = undefined, userText = '', userPhoto = '', userHref = ''}) => {
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
                <time className="published" dateTime={date}>{dateString}</time>
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