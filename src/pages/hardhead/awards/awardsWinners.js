import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import Author from '../../../components/author';

const AwardsWinners = (propsData) => {
    const [data, setData] = useState({ winners: null, isLoading: false, visible: false })

    useEffect(() => {
        const getAwards = async () => {
            var url = config.get('path') + propsData.href;

            if (propsData.position)
                url = url + '?position=' + propsData.position;

            if (propsData.year)
                url = url + '?year=' + propsData.year;

            url = url + '&code=' + config.get('code');

            try {
                setData({ isLoading: true });
                const response = await axios.get(url);
                setData({ winners: response.data, isLoading: false, visible: true });
            } catch (e) {
                console.error(e);
                setData({ isLoading: false, visible: false });
            }
        }

        if (!data.winners) {
            getAwards();
        }
    }, [propsData])

    return (
        <div className="table-wrapper">
            <table>
                <thead>
                    <tr>
                        {propsData.position ?
                            <td width="100px">Ár</td>
                            :
                            <td width="100px">Sæti</td>
                        }
                        <td width="200px">Harðhaus</td>
                        <td width="100px">Atkvæði</td>
                        <td>Útskýring</td>
                    </tr>
                </thead>
                <tbody>
                    {data.visible ?
                        data.winners.map((winner, i) =>
                            <tr key={winner.ID}>
                                <td>{propsData.position ? winner.Year : winner.Position}</td>
                                <td>
                                    {typeof winner.Winner.ProfilePhoto !== 'undefined' ?
                                        <Author ID={winner.Winner.ID} Username={winner.Winner.Username} ProfilePhoto={winner.Winner.ProfilePhoto.Href} /> :
                                        <Author ID={winner.Winner.ID} Username={winner.Winner.Username} />
                                    }
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

export default AwardsWinners;