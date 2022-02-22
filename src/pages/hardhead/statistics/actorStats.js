import axios from "axios";
import { useEffect, useState } from "react"
import config from 'react-global-configuration';
import { Link } from "react-router-dom";
import { Post } from "../../../components";

const ActorStats = (propsData) => {
    const[stats, setStats] = useState();
    const [pageSize, setPageSize] = useState(10);
    const [period, setPeriod] = useState("All");

    var url = config.get('path') + '/api/movies/statistics/actor?periodType=' + period + '&code=' + config.get('code');

    useEffect(() => {
        const getStats = async () => {
            try {
                const response = await axios.get(url);
                setStats(response.data);
            } catch(e) {
                console.error(e);
            }
        }

        getStats();
    }, [propsData, url])

    const handleSubmit = async (event) => {
        console.log(stats.List.length);
        if (pageSize > stats.List.length) {
            setPageSize(10);
        } else {
            setPageSize(pageSize + 10);
        }
    }

    const getButtonText = () => {
        if (stats === undefined || stats === null || pageSize > stats.List.length) {
            return 10;
        }

        return pageSize + 10;
    } 

    const handlePeriodChange = async (event) => {
        setPeriod(event.target.value);
    }  
    
    return (
        <Post
            title="Hvaða leikar hefur oftast verið harðhaus"
            description={
                <select id="demo-category" name="demo-category" onChange={(ev) => handlePeriodChange(ev)}>
                    <option value="All">frá upphafi</option>
                    <option value="Last10">síðustu 10 ár</option>
                    <option value="Last5">síðustu 5 ár</option>
                    <option value="Last2">síðustu 2 ár</option>
                    <option value="ThisYear">þetta ár</option>
                </select>
            }
            date= { stats ? stats.DateFrom : null}
            dateFormatted = { stats ? stats.DateFromString: null}
            showFooter = {false}
            body= { stats ?
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
                            { stats.List.slice(0, pageSize).map((stat, i) =>   
                                <tr key={i}>
                                    <td>{i+1}</td>
                                    <td><Link to={"/hardhead?query=" + stat.Text}>{stat.Text}</Link></td>
                                    <td>{stat.AttendedCount}</td>
                                    <td>{stat.FirstAttendedString}</td>
                                    <td>{stat.LastAttendedString}</td>
                                </tr>              
                            )}
                        </tbody>
                    </table>
                </div>
                : null
            }
            actions={
                <div>
                    <button tooltip="Sjá meira" className="button large" onClick={handleSubmit}>Sjá {getButtonText()} efstu</button>
                </div>
            }
        />
    )    
}

export default ActorStats;