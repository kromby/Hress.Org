import { Link } from 'react-router-dom';
import './intro.css';

const Intro = ({title, description, logo}) => {
    return (
        <section id="intro">
            <Link to="/" className="logo">
                <img src={logo} alt="Hress.Org" />
            </Link>
            <header>
                <h2>{title}</h2>
                <p>{description}</p>
            </header>
        </section>
    )
}

export default Intro;