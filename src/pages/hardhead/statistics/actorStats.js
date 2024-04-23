import axios from "axios";
import { useEffect, useState } from "react"
import config from 'react-global-configuration';
import { Link } from "react-router-dom";
import { Post } from "../../../components";

const ActorStats = () => {
    const [stats, setStats] = useState();
    const [pageSize, setPageSize] = useState(10);
    const [period, setPeriod] = useState("All");
    const [reload, setReload] = useState(false);

    var url = `${config.get('apiPath')}/api/movies/statistics/actor?periodType=${period}`;

    useEffect(() => {
        const getStats = async () => {
            try {
                const response = await axios.get(url);
                setStats(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        if (!stats || reload) {
            getStats();
            setReload(false);
        }
    }, [url])

    const handleSubmit = async () => {
        
        if (pageSize > stats.list.length) {
            setPageSize(10);
        } else {
            setPageSize(pageSize + 10);
        }
    }

    const getButtonText = () => {
        if (stats === undefined || stats === null || pageSize > stats.list.length) {
            return 10;
        }

        return pageSize + 10;
    }

    const handlePeriodChange = async (event) => {
        setPeriod(event.target.value);
        setReload(true);
    }

    return (
        <Post
            title="Hvaða leikari hefur oftast verið harðhaus"
            description={
                <select id="demo-category" name="demo-category" onChange={(ev) => handlePeriodChange(ev)}>
                    <option value="All">frá upphafi</option>
                    <option value="Last10">síðustu 10 ár</option>
                    <option value="Last5">síðustu 5 ár</option>
                    <option value="Last2">síðustu 2 ár</option>
                    <option value="ThisYear">þetta ár</option>
                </select>
            }
            date={stats ? stats.dateFrom : null}
            dateFormatted={stats ? stats.dateFromString : null}
            showFooter={false}
            body={stats ?
                <div className="table-wrapper">
                    <table>
                        <thead>
                            <tr>
                                <td width="100px">Nr.</td>
                                <td width="250px">Leikari</td>
                                <td>Fjöldi</td>
                                <td>Fyrst</td>
                                <td>Síðast</td>
                            </tr>
                        </thead>
                        <tbody>
                            {stats.list.slice(0, pageSize).map((stat, i) =>
                                <tr key={stat.text}>
                                    <td>{i + 1}</td>
                                    <td><Link to={"/hardhead?query=" + stat.text}>{stat.text}</Link></td>
                                    <td>{stat.attendedCount}</td>
                                    <td>{stat.firstAttendedString}</td>
                                    <td>{stat.lastAttendedString}</td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
                : null
            }
            actions={
                <div>
                    <button className="button large" onClick={handleSubmit}>Sjá {getButtonText()} efstu</button>
                </div>
            }
        />
    )
}

export default ActorStats;