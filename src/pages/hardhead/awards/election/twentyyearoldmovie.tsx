import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { Post } from "../../../../components";
import HardheadBody from "../../components/hardheadbody";
import { useAuth } from "../../../../context/auth";
import { HardheadNight } from "../../../../types/hardheadNight";
import { useHardhead } from "../../../../hooks/hardhead/useHardhead";

interface TwentyYearOldMovieProps {
    ID: number;
    Name: string;
    Href: string;
    onSubmit: () => void;
}

const TwentyYearOldMovie = ({ ID, Name, Href, onSubmit }: TwentyYearOldMovieProps) => {
    const { authTokens } = useAuth();
    const [movies, setMovies] = useState<HardheadNight[]>([]);
    const [value, setValue] = useState(-1);
    const { fetchByHref } = useHardhead();

    useEffect(() => {
        const loadNights = async () => {
            const result = await fetchByHref(Href);
            setMovies(result);
        };
        loadNights();
    }, [Href]);

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
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

    const handleChange = (event: number) => {
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
                    key={hardhead.id}
                    id={hardhead.id}
                    title={hardhead.name}
                    date={hardhead.date}
                    dateFormatted={hardhead.dateString}
                    body={<HardheadBody id={hardhead.id} name={hardhead.name} description={hardhead.description} viewNight={false} viewGuests={false} imageHeight={"270px"} movie={hardhead.movie} viewMovie />}
                    actions={
                        <ul className="actions">
                            <div key={hardhead.id} onClick={() => handleChange(hardhead.id)}>
                                <input type="radio" radioGroup={"id_radio"} checked={hardhead.id === value} onChange={() => handleChange(hardhead.id)} />
                                <label>Ég vil sjá þessa mynd</label>
                            </div>
                        </ul>
                    }
                />
            )
                : null}

            <ul className="actions pagination">
                <li>
                    <button onClick={(e) => handleSubmit(e as any)} disabled={value === -1} className="button large next">{"Kjósa " + Name}</button>
                </li>
            </ul>
        </div>
    )
}

export default TwentyYearOldMovie;