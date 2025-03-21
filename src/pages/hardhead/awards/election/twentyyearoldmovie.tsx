import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { Post } from "../../../../components";
import HardheadBody from "../../components/hardheadbody";
import { useAuth } from "../../../../context/auth";
import { HardheadNight } from "../../../../types/hardheadNight";
import { useHardhead } from "../../../../hooks/hardhead/useHardhead";
import { ElectionModuleProps } from ".";
import { useLocation, useNavigate } from "react-router-dom";

const TwentyYearOldMovie = ({ ID, Name, Href, onSubmit }: ElectionModuleProps) => {
    const { authTokens } = useAuth();
    const [movies, setMovies] = useState<HardheadNight[]>([]);
    const [value, setValue] = useState(-1);
    const [error, setError] = useState("");
    const { fetchByHref } = useHardhead();
    const navigate = useNavigate();
    const location = useLocation();

    useEffect(() => {
        const loadNights = async () => {
            const result = await fetchByHref(Href || "");
            setMovies(result);
        };
        loadNights();
    }, [Href]);

    const handleSubmit = async () => {
        if (authTokens === undefined) {
            navigate("/login", { state: { from: location.pathname } });
            return;
        }

        try {
            const url = `${config.get('apiPath')}/api/elections/${ID}/vote`;
            await axios.post(url, [{ EventID: ID, Value: value }], {
                headers: { 'X-Custom-Authorization': `token ${authTokens.token}` },
            });
        } catch (e) {
            console.error(e);
            setError(`Villa við að kjósa mynd ${e}`);
        }

        onSubmit();
    }

    const handleChange = (event: number) => {
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
                {error ? (
                    <li>
                        <b>
                            {error}
                            <br />
                        </b>
                    </li>
                ) : null}
                <li>
                    <button onClick={handleSubmit} disabled={value === -1} className="button large next">{`Kjósa ${Name}`}</button>
                </li>
            </ul>
        </div>
    )
}

export default TwentyYearOldMovie;