import React from 'react';
import { MiniListPost } from '../../../../components';

class UserStatistics extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            isLoaded: false,
            error: null,
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
        var url = 'https://ezhressapi.azurewebsites.net/api/hardhead/statistics/users?periodType=' + localPeriodType + '&code=mIDqQM07DjZa7IkNtkapKigg9Edielksif1ODu49W13p3Xhsf70foQ==';
    
        fetch(url, {
            method: 'GET' 
        })
        .then(res => res.json())
        .then((result) => {
            var list = [];
            result.slice(0,this.state.topN).map(stat =>
                list.push({
                    ID: stat.UserId,
                    Text: stat.AttendedCount + " - (" + new Date(stat.FirstAttended).getFullYear() + " - "  + new Date(stat.LastAttended).getFullYear() + ")",
                    UserID: stat.UserId
                })
            );

            this.setState({error: null, isLoaded: true, stats: result, list: list});
        }, 
        (error) => {
            this.setState({isLoaded: true, error});
        });  
    }

    clickChangeView() {
        if(this.state.topN > 20) {
            this.setState({
                topN: 10,
                buttonText: 'Sjá meira'
            });
        }
        else if(this.state.topN === 10) {
            this.setState({
                topN: 20,
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
        const { error, isLoaded } = this.state;

        if (error) {
            return (
                <table className="body" width="180px" cellspacing="0" cellpadding="0" border="0">
                    {/* <tr><td className="top"><img src={top} alt="layout" /></td></tr> */}
                    <tr>
                        <td valign="top" className="contentData">
                        <div>{error.message}</div>
                        </td>
                    </tr>
                    {/* <tr>
                        <td>
                        <img src={bottom} alt="layout" />
                        </td>
                    </tr> */}
                </table>
            );
        } else if (!isLoaded) {
            return (
                <table className="body" width="180px" cellSpacing="0" cellPadding="0" border="0">
                   <tbody>
                    {/* <tr><td className="top"><img src={top} alt="layout" /></td></tr> */}
                    <tr>
                        <td valign="top" className="contentData">
                            <div>Í vinnslu...</div>


{/* <article className="mini-post">
                    <header>
                        <h3><a href="single.html">Top {this.state.topN} gestir</a></h3>
                        <span className="published">Síðustu 5 árin</span>
                        <select value={this.state.periodType} onChange={this.handlePeriodTypeChange}>
                            <option value="All">Frá upphafi</option>
                            <option value="Last10">Síðustu tíu ár</option>
                            <option value="Last5">Síðustu fimm ár</option>
                            <option value="ThisYear">Þetta ár</option>
                        </select>
                        <time className="published" datetime="2015-10-20">October 20, 2015</time>
                        <a href="#" className="author"><img src="images/avatar.jpg" alt="" /></a>
                        <br/>
                    </header>
                    <div>
                        <ol>
                            {stats.slice(0,this.state.topN).map(stat => (
                                <li key={stat.UserId}>
                                    <UserLink UserId={stat.UserId}/> 
                                    &nbsp;-&nbsp;
                                    {stat.AttendedCount}                        
                                    &nbsp;-&nbsp;
                                    ({new Date(stat.FirstAttended).getFullYear()} - {new Date(stat.LastAttended).getFullYear()})
                                </li>
                            ))}                               
                        </ol>
                        <button className="button fit" onClick={() => this.clickChangeView()}>{this.state.buttonText}</button>
                    </div>
                </article> */}

                        </td>
                    </tr>
                    {/* <tr>
                        <td>
                        <img src={bottom} alt="layout" />
                        </td>
                    </tr> */}
                    </tbody>
                </table>
            );            
        } else {
            return (
                <MiniListPost title={"Top " + this.state.topN + " gestir"} description="Frá upphafi" list={this.state.list}/>                
            )                    
        }
    }
}

export default UserStatistics