import React, {useState, useEffect} from 'react';
import config from 'react-global-configuration';
import { MiniPost } from '../../../components';
import axios from 'axios';

const StatisticSide = (propsData) => {
    const[data, setData] = useState({stats: null, isLoading: false, visible: false})    

    useEffect(() => {
        const getAwards = async () => {
            try {
                const min = 0;
                const max = 4;
                const periodType = Math.round(min + Math.random() * (max - min));

                var url = '';
                if(new Date().getMinutes() % 2 === 0)
                    url = config.get('path') + '/api/hardhead/statistics/users?periodType=' + periodType + '&code=' + config.get('code');
                else
                    url = config.get('path') + '/api/hardhead/statistics/users?guestType=53&periodType=' + periodType + '&code=' + config.get('code');

                setData({isLoading: true});
                const response = await axios.get(url);
                setData({stats: response.data, isLoading: false, visible: true});
            } catch(e) {
                console.error(e);
                setData({isLoading: false, visible: false});
            }
        }



        getAwards();

    }, [propsData])

    const getDescription = (period, guest) => {
        var description = "gestur";

        if(guest === "gestur") 
            description = "Oftast mætt";
        else
            description = "Oftast haldið";

        console.log(period);
        if(period === "All")
            description = description + " frá upphafi";
        else if(period === "Last10")
            description = description + " síðustu 10 árin";
        else if(period === "Last5")
            description = description + " síðustu 5 árin";
        else if(period === "Last2")
            description = description + " síðustu 2 árin";
        else if(period === "ThisYear")
            description = description + " á þessu ári";


        return description;
    }

    return (
        <div>
            {data.visible ?
            <MiniPost title="Tölfræði" href=""
                description={
                    <span>
                        {getDescription(data.stats.PeriodTypeName, data.stats.GuestTypeName)}
                        <br/>
                        {data.stats.List[0].User.Username} - {data.stats.List[0].AttendedCount}
                        <br/>
                        {data.stats.List[0].FirstAttendedString} - {data.stats.List[0].LastAttendedString}
                    </span>
                }
                date={data.stats.DateFrom}
                dateString={data.stats.DateFromString}
                userHref={"http://www.hress.org/Gang/Single.aspx?Id=" + data.stats.List[0].User.Id}
                userPhoto={config.get('path') + data.stats.List[0].User.ProfilePhoto.Href + "?code=" + config.get('code')}
                userText={data.stats.List[0].User.Username} /> :
            null}
        </div>
    )
}

export default StatisticSide;