import { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import AuthorOld from '../../../components/authorOld';

const AwardsWinners = ({href, position, year}) => {
    const [data, setData] = useState({ winners: null, isLoading: false, visible: false })

    useEffect(() => {
        const getAwards = async () => {
            let url = config.get('path') + href;

            if (position)
                url = url + '?position=' + position;

            if (year)
                url = url + '?year=' + year;

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
    }, [href, position, year])

    return (
        <div className="table-wrapper">
            <table>
                <thead>
                    <tr>
                        {position ?
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
                        data.winners.map((winner) =>
                            <tr key={winner.ID}>
                                <td>{position ? winner.Year : winner.Position}</td>
                                <td>
                                    {typeof winner.Winner.ProfilePhoto !== 'undefined' ?
                                        <AuthorOld ID={winner.Winner.ID} Username={winner.Winner.Username} UserPath="/hardhead/users/" ProfilePhoto={winner.Winner.ProfilePhoto.Href} /> :
                                        <AuthorOld ID={winner.Winner.ID} Username={winner.Winner.Username} UserPath="/hardhead/users/" />
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