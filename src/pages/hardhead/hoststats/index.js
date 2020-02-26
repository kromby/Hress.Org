import React from 'react';
import './index.css';
import UserLink from '../../../components/users/userlink.js';

import top from './top_small.png';
import bottom from './bottom_small.png';

class HostStatistics extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            isLoaded: false,
            error: null,
            stats: null,
            topN: 5,
            buttonText: 'Sjá meira', 
            periodType: 'All'
        }

        this.handlePeriodTypeChange = this.handlePeriodTypeChange.bind(this);
    }

    componentDidMount() {
        this.getData(this.state.periodType);
    }

    getData(localPeriodType) {
        var url = 'https://ezhressapi.azurewebsites.net/api/hardhead/statistics/users?guestType=53&periodType=' + localPeriodType + '&code=mIDqQM07DjZa7IkNtkapKigg9Edielksif1ODu49W13p3Xhsf70foQ==';
    
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

    clickChangeView() {
        if(this.state.topN > 10) {
            this.setState({
                topN: 5,
                buttonText: 'Sjá meira'
            });
        }
        else if(this.state.topN === 5) {
            this.setState({
                topN: 10,
                buttonText: 'Sjá ennþá meira'
            });
        }
        else {
            this.setState({
                topN: this.state.stats.length,
                buttonText: 'Sjá minna'
            });   
        }        
    }

    handlePeriodTypeChange(event) {
        this.setState({periodType: event.target.value});
        this.getData(event.target.value);
    }

    render() {
        const { error, isLoaded, stats } = this.state;

        var topCounter = 1;

        const monthNames = ["janúar", "febrúar", "mars", "apríl", "maí", "júní",
                            "júlí", "ágúst", "september", "október", "nóvember", "desember"
];

        if (error) {
            return (
                <table className="body" width="180px" cellspacing="0" cellpadding="0" border="0">
                    <tr><td className="top"><img src={top} alt="layout" /></td></tr>
                    <tr>
                        <td valign="top" className="contentData">
                        <div>{error.message}</div>
                        </td>
                    </tr>
                    <tr>
                        <td>
                        <img src={bottom} alt="layout" />
                        </td>
                    </tr>
                </table>
            );
        } else if (!isLoaded) {
            return (
                <table className="body" width="180px" cellspacing="0" cellpadding="0" border="0">
                    <tr><td className="top"><img src={top} alt="layout" /></td></tr>
                    <tr>
                        <td valign="top" className="contentData">
                            <div>Í vinnslu...</div>
                        </td>
                    </tr>
                    <tr>
                        <td>
                        <img src={bottom} alt="layout" />
                        </td>
                    </tr>
                </table>
            );            
        } else {
            return (
                <table className="body" width="180px" cellspacing="0" cellpadding="0" border="0">
                    <tr><td className="top"><img src={top} alt="layout" /></td></tr>
                    <tr>
                        <th className="contentData">
                            Top {this.state.topN} gestgjafar
                        </th>
                    </tr>
                    <tr>
                        <td className="contentData">
                            <select value={this.state.periodType} onChange={this.handlePeriodTypeChange}>
                                <option value="All">Frá upphafi</option>
                                <option value="Last10">Síðustu tíu ár</option>
                                <option value="Last5">Síðustu fimm ár</option>
                                <option value="ThisYear">Þetta ár</option>
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <td className="contentData">
                            <u>
                                Harðhaus - Fjöldi
                                <br/>
                                Fyrst haldið - Síðast haldið
                            </u>
                        </td>
                    </tr>
                    <tr>
                        <td valign="top" className="contentData">
                            {stats.slice(0,this.state.topN).map(stat => (
                                <div key={stat.UserId}>
                                    {topCounter++}. <UserLink UserId={stat.UserId}/> - {stat.AttendedCount}                                    
                                    <br/>
                                     {monthNames[new Date(stat.FirstAttended).getMonth()]} {new Date(stat.FirstAttended).getFullYear()}
                                     <br/>
                                     {monthNames[new Date(stat.LastAttended).getMonth()]} {new Date(stat.LastAttended).getFullYear()}
                                </div>
                            ))}
                            <br/>
                            {/* <a href="/smu">Sjá meira</a> */}
                            <button onClick={() => this.clickChangeView()}>{this.state.buttonText}</button>
                        </td>
                    </tr>
                    <tr>
                        <td>
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

export default HostStatistics