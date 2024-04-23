import { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import { Post } from '../../../components';
import Author from '../../../components/author';

const HostStats = () => {
    const [data, setData] = useState({ stats: null, isLoading: false, visible: false })
    const [pageSize, setPageSize] = useState(10);
    const [period, setPeriod] = useState("All");
    const [reload, setReload] = useState(false);

    var url = config.get('apiPath') + '/api/hardhead/statistics/users?attendanceType=53&periodType=' + period;

    useEffect(() => {
        const getStats = async () => {
            try {
                setData({ isLoading: true });
                const response = await axios.get(url);
                setData({ stats: response.data, isLoading: false, visible: true });
            } catch (e) {
                console.error(e);
                setData({ isLoading: false, visible: false })
            }
        }

        if (!data.stats || reload) {
            getStats();
            setReload(false);
        }
    }, [url])

    const handleSubmit = async () => {
        
        if (pageSize > data.stats.list.length) {
            setPageSize(10);
        } else {
            setPageSize(pageSize + 10);
        }
    }

    const getButtonText = () => {
        if (data.stats === undefined || data.stats === null || pageSize > data.stats.list.length) {
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
            title="Hver hefur haldið flest harðhausakvöld"
            description={
                <select id="demo-category" name="demo-category" onChange={(ev) => handlePeriodChange(ev)}>
                    <option value="All">frá upphafi</option>
                    <option value="Last10">síðustu 10 ár</option>
                    <option value="Last5">síðustu 5 ár</option>
                    <option value="Last2">síðustu 2 ár</option>
                    <option value="ThisYear">þetta ár</option>
                </select>
            }
            date={data.visible ? data.stats.dateFrom : null}
            dateFormatted={data.visible ? data.stats.dateFromString : null}
            showFooter={false}
            body={data.visible ?
                <div className="table-wrapper">
                    <table>
                        <thead>
                            <tr>
                                <td width="100px">Nr.</td>
                                <td width="250px">Harðhaus</td>
                                <td>Fjöldi</td>
                                <td>Fyrst haldið</td>
                                <td>Síðast haldið</td>
                            </tr>
                        </thead>
                        <tbody>
                            {data.stats.list.slice(0, pageSize).map((stat, i) =>
                                <tr key={stat.user.id}>
                                    <td>{i + 1}</td>
                                    <td>
                                        {typeof stat.user.profilePhoto !== 'undefined' && stat.user.profilePhoto !== null ?
                                            <Author ID={stat.user.id} Username={stat.user.username} UserPath="/hardhead/users/" ProfilePhoto={stat.user.profilePhoto.href} /> :
                                            <Author ID={stat.user.id} Username={stat.user.username} UserPath="/hardhead/users/" />
                                        }
                                    </td>
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

export default HostStats;