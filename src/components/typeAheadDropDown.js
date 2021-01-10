import React, { useEffect, useState } from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import './typeAheadDropDown.css';

const TypeAheadDropDown = (propsData) => {
    const [text, setText] = useState("");
    const [minLength, setMinLength] = useState(3);
    const [suggestions, setSuggestions] = useState();
    const [placeholder, setPlaceholder] = useState("Sláðu inn gildi");    

    useEffect(() => {
        if (propsData.placeholder) {
            setPlaceholder(propsData.placeholder);
        }
        if (!minLength && propsData.minimum) {
            setMinLength(propsData.minimum);
        }
        if (!text && propsData.defaultValue) {
            setText(propsData.defaultValue);
        }
    })

    const onTextChange = async (e) => {
        const value = e.target.value;
        setText(value);

        console.log("min= " + minLength);
        console.log(value.length);

        if (value.length >= minLength) {
            try {
                const response = await axios.get("http://www.omdbapi.com/?apikey=" + config.get("omdb") + "&s=" + value + "&type=movie");
                setSuggestions(response.data);
            } catch (e) {
                console.error(e);
            }
        }
    }

    const onSelect = (id, name) => {
        console.log("onSelect");
        console.log(id);
        console.log(name);

        setText(name);
        setSuggestions(null);

        propsData.callback(id);
    }

    const renderSuggestion = () => {
        if (suggestions && suggestions.Search) {
            console.log(suggestions);
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
        } else if (suggestions && suggestions.Error) {
            return (
                <i>{suggestions.Error}</i>);
        }
    }

    return (
        <div className={suggestions ? "TypeAheadDropDown" : null}>
            <input id="name" type="text" name="name" onChange={(ev) => onTextChange(ev)} value={text} placeholder={placeholder} />
            {renderSuggestion()}
        </div>
    );
}

export default TypeAheadDropDown;