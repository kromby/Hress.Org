import React from 'react';
import './index.css';
import UserLink from '../../../components/users/userlink.js';

import top from './top_small.png';
import bottom from './bottom_small.png';

class UserStatistics extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            isLoaded: false,
            error: null,
            stats: null
        }
    }

    componentDidMount() {
        var url = 'https://ezhressapi.azurewebsites.net/api/hardhead/statistics/users?code=mIDqQM07DjZa7IkNtkapKigg9Edielksif1ODu49W13p3Xhsf70foQ==';
    
        fetch(url, {
            method: 'GET' 
        })
        .then(res => res.json())
        .then((result) => {
            this.setState({error: null, isLoaded: true, stats: result});
        }, 
        (error) => {
            this.setState({isLoaded: true, error});
        });  
    }

    render() {
        const { error, isLoaded, stats } = this.state;

        var topCounter = 1;

        const monthNames = ["janúar", "febrúar", "mars", "apríl", "maí", "júní",
                            "júlí", "ágúst", "september", "október", "nóvember", "desember"
];

        if (error) {
            return <div>{error.message}</div>;
        } else if (!isLoaded) {
            return <div>Í vinnslu...</div>;
        } else {
            return (
                <table className="body" width="180px" cellspacing="0" cellpadding="0" border="0">
                    <tr><td colspan="3"><img src={top} alt="layout" /></td></tr>
                    <tr>
                        <td className="MiddleLeft" />
                        <td valign="top" className="contentData">
                            <p>
                                Top 10 gestir
                            </p>
                            <div>
                                <u>
                                    Harðhaus - Fjöldi
                                    <br/>
                                    Fyrst mætt - Síðast mætt
                                </u>
                            </div>
                            <br/>
                            {stats.slice(0,3).map(stat => (
                                <div key={stat.UserId}>
                                    <b>
                                    {topCounter++}. <UserLink UserId={stat.UserId}/> - {stat.AttendedCount}                                    
                                    </b>
                                    <br/>
                                     {monthNames[new Date(stat.FirstAttended).getMonth()]} {new Date(stat.FirstAttended).getFullYear()}
                                     <br/>
                                     {monthNames[new Date(stat.LastAttended).getMonth()]} {new Date(stat.LastAttended).getFullYear()}
                                </div>
                            ))}
                            {stats.slice(3,15).map(stat => (
                                <div key={stat.UserId}>
                                    {topCounter++}. <UserLink UserId={stat.UserId}/> - {stat.AttendedCount} 
                                    <br/>
                                    {new Date(stat.FirstAttended).getFullYear()} - {new Date(stat.LastAttended).getFullYear()}
                                </div>
                            ))}
                            <br/>
                            {/* <a href="/smu">Sjá meira</a> */}
                        </td>
                        <td class="MiddleRight" />
                    </tr>
                    <tr>
                        <td colspan="3">
                        <img src={bottom} alt="layout" />
                        </td>
                    </tr>
                    <tr>
                        <td className="gap" />
                    </tr>
                </table>
            )                    
        }
    }
}

export default UserStatistics