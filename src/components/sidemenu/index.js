import React from 'react';
import UserStatistics from '../hardhead/userstats';
import HostStatistics from '../hardhead/hoststats';

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