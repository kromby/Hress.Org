import axios from "axios";
import { useEffect, useState } from "react";
import config from 'react-global-configuration';
import { Post } from "../../components";
import { useAuth } from "../../context/auth";


const Profile = (propsData) => {
    const { authTokens } = useAuth();
    const [balanceSheet, setBalanceSheet] = useState();

    useEffect(() => {
        const getBalanceSheet = async () => {
            var url = config.get("apiPath") + "/api/users/0/balancesheet";
            try {
                const response = await axios.get(url, {
                    headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
                });
                setBalanceSheet(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        document.title = "Fjármál | Hress.Org";

        if (!balanceSheet) {
            getBalanceSheet();
        }
    }, [propsData])

    return (
        <div id="main">
            {balanceSheet ?
                <Post
                    title="Hress sjóðurinn"
                    description="Reikningsnr.: 1187-15-200315 - Kt: 090979-3029"
                    body={
                        <section>
                            <h3 key="Header3">Útistandandi færslur</h3>
                            <div className="table-wrapper" key="Table4">
                                <table>
                                    <thead>
                                        <tr>
                                            <th width="200px">Dagsetning</th>
                                            <th width="900px">Útskýring</th>
                                            <th>Upphæð</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {balanceSheet.transactions.map(transaction =>
                                            <tr key={transaction.id}>
                                                <td>{transaction.insertedString}</td>
                                                <td>{transaction.name}</td>
                                                <td>{transaction.amount}</td>
                                            </tr>
                                        )}
                                        <tr>
                                            <td></td>
                                            <td><b>Samtals</b></td>
                                            <td><b>{balanceSheet.balance}</b></td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </section>
                    }
                /> : null}
        </div>
    )
}

export default Profile;