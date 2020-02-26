import React from 'react';
import UserStatistics from '../../pages/hardhead/userstats';
import HostStatistics from '../../pages/hardhead/hoststats';

class SideMenu extends React.Component {
    render() {        
        return (
            <div>
                <UserStatistics />
                <br/>
                <HostStatistics />
            </div>
        )
    }
}

export default SideMenu