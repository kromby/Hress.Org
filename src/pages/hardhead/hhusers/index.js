import axios from "axios";
import { useEffect, useState } from "react"
import { Post } from "../../../components";
import config from 'react-global-configuration';
import UserAwards from "./userAwards";

const HHUsers = (propsData) => { 
    // const[user, setUser] = useState();
    // const[stats, setStats]    = useState();

    // useEffect(() => {
    //     const getProfile = async () => {
    //         var url = config.get('path') + '/api/users/' + propsData.match.params.id + '/?code=' + config.get('code');

    //         try {
    //             const response = await axios.get(url);
    //             setUser(response.data);
    //         } catch(e) {
    //             console.error(e);
    //         }            
    //     }

    //     const getStats = async () => {
    //         var url = config.get('path') + '/api/hardhead/statistics/users/' + propsData.match.params.id + '?periodType=All&code=' + config.get('code');

    //         try {
    //             const response = await axios.get(url);
    //             setStats(response.data.List[0]);
    //         } catch(e) {
    //             console.error(e);
    //         }            
    //     }

    //     getProfile();  
    //     getStats();      

    // }, [propsData])


    return (
        <div id="main">
            {/* {user ? 
                <Post key={user.ID}
                    id={user.ID}
                    title={user.Name}
                    description={stats ? "Hefur mætt á " + stats.AttendedCount + " kvöld" : null}
                    author={user}
                    date={stats ? stats.FirstAttended : null}
					dateFormatted={stats ? stats.FirstAttendedString : null} 
                />
                
            : null} */}
            <UserAwards id={propsData.match.params.id} />
        </div>
    )
}

export default HHUsers;