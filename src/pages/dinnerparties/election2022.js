import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import axios from "axios";
import { Post } from "../../components";
import { Link } from "react-router-dom";

const Election2022 = (propsData) => {
    const [courses, setCourses] = useState();
    const [selected, setSelected] = useState();
    const [savingAllowed, setSavingAllowed] = useState(false);
    const [message, setMessage] = useState();

    useEffect(() => {
        const getCourses = async () => {
            var url = config.get("apiPath") + "/api/dinnerparties/courses/" + propsData.match.params.typeID;
            try {
                const response = await axios.get(url);
                setCourses(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        document.title = "Hress.Org | Hress.Org";

        if (!courses) {
            getCourses();
        }
    }, [propsData]);

    const handleChange = async (event) => {
        setSelected(event);
        setSavingAllowed(true);
    }

    const handleSubmit = async (event) => {
        setSavingAllowed(false);
        event.preventDefault();

        setMessage("Atkvæði vistað! (" + selected + ")");
    }

    return (
        <div id="main">
            <Post
                title="Kosning um forrétt"
                description="Veldu þann rétt sem þú vilt fá á næsta Matar- og Rauðvínskvöldi"
                dateFormatted="2003-2021"
                body={
                    <div className="row gtr-uniform">
                        {courses ? courses.map(course =>
                            <div className="col-4" key={course.id} onClick={() => handleChange(course.id)} >
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