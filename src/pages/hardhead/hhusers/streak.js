import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { Post } from "../../../components";

const Streak = ({ id }) => {
    const [streaks, setStreaks] = useState();

    useEffect(() => {
        const getStreaks = async () => {
            var url = new URL(`/api/hardhead/statistics/users/${id}/streaks`, config.get('apiPath'));
            try {
                const response = await axios.get(url);
                setStreaks(response.data);
            } catch (e) {
                console.error("[Streak.js] Error fetching data: ", e);
            }
        }

        if (!streaks) {
            getStreaks();
        }
    }, []);

    return (
        <div id="main">
            <Post
                title="Kvöldrunur"
                description=""
                body={
                    <div className="table-wrapper">
                        <table>
                            <thead>
                                <tr>
                                    <th>Frá</th>
                                    <th>Til</th>
                                    <th>Runa</th>
                                </tr>
                            </thead>
                            <tbody>
                                {streaks ? streaks.slice(0, 10).map((streak, i) =>
                                    <tr key={i}>
                                        <td>{streak.firstAttendedString}</td>
                                        <td>{streak.lastAttendedString}</td>
                                        <td>{streak.attendedCount}</td>
                                    </tr>
                                ) : null}
                            </tbody>
                        </table>
                    </div>
                }
            />
        </div>
    )
}

export default Streak;