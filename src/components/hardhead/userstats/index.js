import React from 'react';
import './index.css';

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
        var url = 'http://localhost:7071/api/hardhead/statistics/users';
    
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
                    <tr>
                        <td colspan="3">
                            <img src={top} alt="layout" />
                        </td>
                    </tr>
                    <tr>
                        <td className="MiddleLeft" />
                        <td valign="top" className="contentData">
                            <p>
                                Top 10 gestir
                            </p>
                            <div>
                                Harðhaus - Fjöldi - Fyrst mætt
                            </div>
                            {stats.slice(0,5).map(stat => (
                                <div key={stat.UserId}>
                                    <b>
                                    <a href="/smu">{stat.UserId}</a>
                                     - 
                                    {stat.AttendedCount}
                                     - 
                                    {monthNames[new Date(stat.FirstAttended).getMonth()]} {new Date(stat.FirstAttended).getFullYear()}
                                    </b>
                                </div>
                            ))}
                            {stats.slice(5,15).map(stat => (
                                <div key={stat.UserId}>
                                    <a href="/smu">{stat.UserId}</a>
                                     - 
                                    {stat.AttendedCount}
                                     - 
                                    {monthNames[new Date(stat.FirstAttended).getMonth()]} {new Date(stat.FirstAttended).getFullYear()}
                                </div>
                            ))}
                            <br/>
                            <a href="/smu">Sjá meira</a>
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