import React, {useState, useEffect} from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import { Post } from '../../../components';
import Author from '../../../components/author';

const HostStats = (propsData) => {
    const[data, setData] = useState({stats: null, isLoading: false, visible: false})
    const [pageSize, setPageSize] = useState(10);

    var url = config.get('path') + '/api/hardhead/statistics/users?guestType=53&periodType=All&code=' + config.get('code');	
    
    useEffect(() => {
        const getStats = async () => {
            try {
                setData({isLoading: true});
                const response = await axios.get(url);
                setData({stats: response.data, isLoading: false, visible: true});
            } catch(e) {
                console.error(e);
                setData({isLoading: false, visible: false})
            }
        }

        getStats();
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

    return (
        <Post
            title="Hver hefur haldið flest harðhausakvöld"
            description="frá upphafi"
            date= { data.visible ? data.stats.DateFrom : null}
            dateFormatted = { data.visible ? data.stats.DateFromString: null}
            showFooter = {false}
            body= { data.visible ?
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
                            { data.stats.List.slice(0, pageSize).map((stat, i) =>   
                                <tr key={i}>
                                    <td>{i+1}</td>
                                    <td>
                                        {typeof stat.User.ProfilePhoto !=='undefined' && stat.User.ProfilePhoto !== null ?
                                            <Author ID={stat.User.ID} Username={stat.User.Username} ProfilePhoto={stat.User.ProfilePhoto.Href} /> :
                                            <Author ID={stat.User.ID} Username={stat.User.Username} />
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