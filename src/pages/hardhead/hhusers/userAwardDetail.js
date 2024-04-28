import { useEffect, useState } from "react";
import axios from "axios";
import config from 'react-global-configuration';

const UserAwardDetail = ({awardID, userID, name}) => {
    const [awards, setAwards] = useState();

    useEffect(() => {
        const getAwards = async () => {
            const url = config.get('path') + '/api/hardhead/awards/' + awardID + '/winners?user=' + userID + '&code=' + config.get('code');
            try {
                const response = await axios.get(url);
                if (response.data.length > 0) {
                    setAwards(response.data);
                }
            } catch (e) {
                console.error(e);
            }
        }

        if (!awards) {
            getAwards();
        }
    }, [awardID, userID])

    return (<div>{awards ? <div>
        <h2>{name}</h2>
        <div className="table-wrapper">
            <table>
                <thead>
                    <tr>
                        <td width="100px">Ár</td>
                        <td width="100px">Sæti</td>
                        <td width="100px">Atkvæði</td>
                        <td>Útskýring</td>
                    </tr>
                </thead>
                <tbody>
                    {awards.filter(a => a.Position <= 3).map(award =>
                        <tr key={award.ID} style={award.Position === 1 ? { backgroundColor: "#FBEE99" } : award.Position === 2 ? { backgroundColor: "#F5F5F5" } : award.Position === 3 ? { backgroundColor: "#FEE6E1" } : null}>
                            <td>{award.Year}</td>
                            <td>{award.Position}</td>
                            <td>{award.Value}</td>
                            <td>{award.Text}</td>
                        </tr>)}
                </tbody>
            </table>
        </div>
    </div> : null}</div>);
}

export default UserAwardDetail;