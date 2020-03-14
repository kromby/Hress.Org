import React, {useState, useEffect} from 'react';
import config from 'react-global-configuration';
import axios from 'axios';

const AwardWinners = (propsData) => {
    const[data, setData] = useState({winners: null, isLoading: false, visible: false})    		

    useEffect(() => {
        const getAwards = async () => {
            var url = config.get('path') + propsData.href + '?code=' + config.get('code');

            try {
                setData({isLoading: true});
                const response = await axios.get(url);
                setData({winners: response.data, isLoading: false, visible: true});
            } catch(e) {
                console.error(e);
                setData({isLoading: false, visible: false});
            }
        }

        getAwards();

    }, [propsData])    

    return (
        <div className="table-wrapper">
            <table>
                <thead>
                    <tr>
                        <td width="80px">Ár</td>
                        <td width="80px">Sæti</td>
                        <td width="120px">Harðhaus</td>
                        <td width="80px">Atkvæði</td>
                        <td>Útskýring</td>
                    </tr>
                </thead>
                <tbody>
                    {data.visible ?
                        data.winners.map((winner, i) =>                 
                            <tr key={winner.ID}>
                                <td>{winner.Year}</td>
                                <td>{winner.Position}</td>
                                <td>
                                    <a href={winner.Winner.ID} className="author">
                                        {winner.Winner.ProfilePhoto ? <img src={config.get('path') + winner.Winner.ProfilePhoto.Href + '?code=' + config.get('code')} alt={winner.Winner.Username} /> : winner.Winner.Username}
                                    </a>
                                </td>
                                
                                <td>
                                {winner.Value}
                                </td>
                                <td>
                                {winner.Text}
                                </td>
                            </tr>
                        ) : 
                    null}
                </tbody>
            </table>
        </div>
    )
}

export default AwardWinners;