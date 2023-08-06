import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import { Post } from '../../../components';
import Author from '../../../components/author';

const HostStats = (propsData) => {
    const [data, setData] = useState({ stats: null, isLoading: false, visible: false })
    const [pageSize, setPageSize] = useState(10);
    const [period, setPeriod] = useState("All");
    const [reload, setReload] = useState(false);

    var url = config.get('path') + '/api/hardhead/statistics/users?guestType=53&periodType=' + period + '&code=' + config.get('code');

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
    }, [propsData, url])

    const handleSubmit = async (event) => {
        console.log(data.stats.List.length);
        if (pageSize > data.stats.List.length) {
            setPageSize(10);
        } else {
            setPageSize(pageSize + 10);
        }
    }

    const getButtonText = () => {
        if (data.stats === undefined || data.stats === null || pageSize > data.stats.List.length) {
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
            date={data.visible ? data.stats.DateFrom : null}
            dateFormatted={data.visible ? data.stats.DateFromString : null}
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
                            {data.stats.List.slice(0, pageSize).map((stat, i) =>
                                <tr key={i}>
                                    <td>{i + 1}</td>
                                    <td>
                                        {typeof stat.User.ProfilePhoto !== 'undefined' && stat.User.ProfilePhoto !== null ?
                                            <Author ID={stat.User.ID} Username={stat.User.Username} UserPath="/hardhead/users/" ProfilePhoto={stat.User.ProfilePhoto.Href} /> :
                                            <Author ID={stat.User.ID} Username={stat.User.Username} UserPath="/hardhead/users/" />
                                        }
                                    </td>
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

export default HostStats;