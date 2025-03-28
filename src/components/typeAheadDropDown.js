import { useEffect, useState } from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import './typeAheadDropDown.css';

const TypeAheadDropDown = ({minimum, defaultValue, placeholder, callback}) => {
    const [text, setText] = useState("");
    const [minLength, setMinLength] = useState(3);
    const [suggestions, setSuggestions] = useState();
    const [placeholderText, setPlaceholderText] = useState("Sláðu inn gildi");    

    useEffect(() => {
        if (placeholder) {
            setPlaceholderText(placeholder);
        }
        if (!minLength && minimum) {
            setMinLength(minimum);
        }
        if (!text && defaultValue) {
            setText(defaultValue);
        }
    })

    const onTextChange = async (e) => {
        const value = e.target.value;
        setText(value);

        if (value.length >= minLength) {
            try {
                const response = await axios.get("https://www.omdbapi.com/?apikey=" + config.get("omdb") + "&s=" + value + "&type=movie");
                setSuggestions(response.data);
            } catch (err) {
                console.error(err);
            }
        }
    }

    const onSelect = (id, name) => {
        setText(name);
        setSuggestions(null);

        callback(id);
    }

    const renderSuggestion = () => {
        if (suggestions?.Search) {
            
            return (
                <ul>
                    {suggestions.Search.map(movie =>
                        <li key={movie.imdbID} onClick={() => onSelect(movie.imdbID, movie.Title)}>
                            {movie.Poster !== "N/A" ?
                                <img src={movie.Poster} alt={movie.Title} />
                                : null}&nbsp;
                            {movie.Title} ({movie.Year})                            
                        </li>)}
                </ul>
            );
        } else if (suggestions?.Error) {
            return (
                <i>{suggestions.Error}</i>);
        }
    }

    return (
        <div className={suggestions ? "TypeAheadDropDown" : null}>
            <input id="name" type="text" name="name" onChange={(ev) => onTextChange(ev)} value={text} placeholder={placeholderText} />
            {renderSuggestion()}
        </div>
    );
}

export default TypeAheadDropDown;