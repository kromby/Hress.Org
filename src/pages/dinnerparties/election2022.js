import { useEffect, useState } from "react";
import { Link, useLocation } from 'react-router-dom';
import config from 'react-global-configuration';
import axios from "axios";
import {isMobile} from 'react-device-detect';
import { Post } from "../../components";
import { useAuth } from "../../context/auth";

const Election2022 = (propsData) => {
    const { authTokens } = useAuth();
    const [courses, setCourses] = useState();
    const [selected, setSelected] = useState();
    const [savingAllowed, setSavingAllowed] = useState(false);
    const [message, setMessage] = useState();
    const { pathname } = useLocation();

    var url = config.get("apiPath") + "/api/dinnerparties/courses/" + propsData.match.params.typeID;

    useEffect(() => {
        const getCourses = async () => {
            try {
                const response = await axios.get(url);
                setCourses(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        document.title = "Kosning fyrir Matar- og Rauðvíns | Hress.Org";

        if (!courses) {
            getCourses();
        }
    }, [propsData, url]);

    const handleChange = async (event) => {
        setSelected(event);
        setSavingAllowed(true);
    }

    const handleSubmit = async (event) => {
        setSavingAllowed(false);
        event.preventDefault();

        try {
            await axios.post(url, {
                courseID: selected
            }, {
                headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
            });
            setMessage("Atkvæði vistað!");
        } catch (e) {
            console.error(e);
            if (e.response && e.response.status === 400) {
                setMessage("Ekki tókst að kjósa! - " + e.message);
            }
            else {
                setMessage("Ekki tókst að kjósa!");
            }
        }
    }

    if (!authTokens) {
        return (
            <div id="main">
                <Post
                    title="Kosning um eftirrétt"
                    body={
                        <p>Þú verður að vera <Link to={{ pathname: "/login", state: { from: pathname } }}>skráð/ur inn</Link> til þess að taka þátt í kosningunni.</p>
                    }
                />
            </div>
        );
    }

    return (
        <div id="main">
            <Post
                title="Kosning um eftirrétt"
                description="Veldu þann rétt sem þú vilt fá á næsta Matar- og Rauðvínskvöldi"
                dateFormatted="2003-2021"
                body={
                    <div className="row gtr-uniform">
                        {courses ? courses.map(course =>
                            <div className={isMobile ? "col-12" : "col-4"} key={course.id} onClick={() => handleChange(course.id)} >
                                <input type="radio" checked={selected === course.id} onChange={() => handleChange(course.id)} />
                                <label>
                                    <b dangerouslySetInnerHTML={{ __html: course.name }} />
                                    <br />
                                    <Link to={"/dinnerparties/" + course.eventID} target="_blank">Var á Matar- og Rauðvíns árið {course.year}</Link>
                                </label>
                            </div>
                        ) : null}
                        <div className="col-12">
                            <br />
                        </div>
                    </div>
                }
                actions={
                    <ul className="actions">
                        <li>
                            <button className="button large" onClick={handleSubmit} disabled={!savingAllowed}>Kjósa</button>
                        </li>
                    </ul>
                }
                stats={<b>{message}</b>}
            />
        </div>
    )
}

export default Election2022;