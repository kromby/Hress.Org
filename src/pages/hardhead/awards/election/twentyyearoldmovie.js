import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { Post } from "../../../../components";
import HardheadBody from "../../components/hardheadbody";
import { useAuth } from "../../../../context/auth";

const TwentyYearOldMovie = ({ ID, Name, Href, onSubmit }) => {
    const { authTokens } = useAuth();
    const [movies, setMovies] = useState();
    const [value, setValue] = useState(-1);

    useEffect(() => {
        const getMovies = async () => {
            // TODO: Get parent ID from somewhere or just change every year ;)
            var url = config.get('path') + Href + '&code=' + config.get('code');
            try {
                const response = await axios.get(url);
                setMovies(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        if (!movies) {
            getMovies();
        }
    }, []);

    const handleSubmit = async (event) => {
        event.preventDefault();
        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        if (value === -1) {
            alert("Engin mynd valin!");
            return;
        }

        try {
            const url = config.get('apiPath') + '/api/elections/' + ID + '/vote';
            await axios.post(url, [{ EventID: ID, Value: value }], {
                headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
            });
        } catch (e) {
            console.error(e);
            alert(e);
        }

        onSubmit();
    }

    const handleChange = (event) => {
        if (authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        setValue(event);
    }

    return (
        <div>
            <Post
                id={ID}
                title={Name}
                body={
                    <section>
                        <p>
                            Veldu þá mynd sem þig langar til að sjá á uppgjörskvöldinu í janúar, smelltu síðan á <b>Kjósa</b> neðst á síðunni til að halda áfram
                        </p>
                    </section>
                }
            />
            {movies ? movies.map(hardhead =>
                <Post
                    key={hardhead.ID}
                    id={hardhead.ID}
                    title={hardhead.Name}
                    date={hardhead.Date}
                    dateFormatted={hardhead.DateString}
                    // body= { <Movie id={hardhead.ID}/> }
                    body={<HardheadBody id={hardhead.ID} name={hardhead.Name} description={hardhead.Description} viewNight={false} viewGuests={false} imageHeight={"270px"} />}
                    actions={
                        <ul className="actions">
                            <div key={hardhead.ID} onClick={() => handleChange(hardhead.ID)}>
                                <input type="radio" radioGroup={"id_radio"} checked={hardhead.ID === value} onChange={() => handleChange(hardhead.ID)} />
                                <label>Ég vil sjá þessa mynd</label>
                            </div>
                        </ul>
                    }
                />
            )
                : null}

            <ul className="actions pagination">
                <li>
                    <button onClick={handleSubmit} disabled={value === -1} className="button large next">{"Kjósa " + Name}</button>
                </li>
            </ul>
        </div>
    )
}

export default TwentyYearOldMovie;