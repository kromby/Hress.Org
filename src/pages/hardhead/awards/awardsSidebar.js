import { useEffect, useState } from "react"
import config from 'react-global-configuration';
import axios from 'axios';
import SidePost from "../../../components/sidepost";

const AwardsSidebar = () => {
    const [years, setYears] = useState();

    var url = config.get('path') + '/api/hardhead/years?code=' + config.get('code');

    useEffect(() => {
        const getYears = async () => {
            try {
                const response = await axios.get(url);
                setYears(response.data.filter(year => year.Hardhead));
            } catch (e) {
                console.error(e);
            }
        }

        if (!years) {
            getYears();
        }
    }, [url])

    return (
        <section id="sidebar">
            <section>
                <ul className="posts">
                    {years ? years.map((year) =>
                        <li key={year.ID}>
                            <SidePost
                                key={year.ID}
                                title={"Árið " + year.Name}
                                href={"/hardhead/awards/year/" + year.ID}
                                dateString={"Harðhaus ársins " + year.Hardhead.Username}
                                image={year.Hardhead ? config.get('apiPath') + year.Hardhead.ProfilePhoto.Href : null}
                                imageText={year.Hardhead ? "Harðhaus ársins: " + year.Hardhead.Username : null} />
                        </li>
                    ) : null}
                </ul>
            </section>
        </section>
    )
}

export default AwardsSidebar;