import { useEffect, useState } from "react";
import config from "react-global-configuration";
import axios from "axios";
import { Post } from "../../../components";


const UserStatistics = ({ id }) => {
    const [userStatistics, setUserStatistics] = useState([]);

    useEffect(() => {
        const getUserStatistics = async (attendanceType, period) => {
            const url = `${config.get('apiPath')}/api/hardhead/statistics/users/${id}?attendanceType=${attendanceType}&periodType=${period}`;
            try {
                const response = await axios.get(url);
                return response.data;
            } catch (e) {
                console.error(e);
            }
        }

        const getData = async () => {
            let tempArray = [];
            for (let i = 0; i <= 4; i++) {
                const result = await getUserStatistics(52, i);
                tempArray = [...tempArray, result];
                setUserStatistics(tempArray);
            }

            for (let i = 0; i <= 4; i++) {
                const result = await getUserStatistics(53, i);
                tempArray = [...tempArray, result];
                setUserStatistics(tempArray);
            }

            
        }

        if (userStatistics.length < 1) {
            getData();
        }
    }, [id]);

    const getPeriodTypeName = (period) => {
        switch (period) {
            case "All":
                return "frá upphafi";
            case "Last10":
                return "síðustu 10 ár";
            case "Last5":
                return "síðustu 5 ár";
            case "Last2":
                return "síðustu 2 ár";
            case "ThisYear":
                return "þetta ár";
            default:
                return "fleh";
        }
    }

    return (
        <div id="main">
            <Post
                title="Tölfræðin"
                // description="?"
                // dateFormatted="?"
                body={
                    <div className="table-wrapper">
                        <table>
                            <thead>
                                <tr>
                                    <th>Tegund</th>
                                    <th>Fjöldi</th>
                                    <th>Fyrsta</th>
                                </tr>
                            </thead>
                            <tbody>
                                {userStatistics ? userStatistics.map((stat) => 
                                <tr key={stat.typeName}>
                                    <td>{stat.typeName + " " + getPeriodTypeName(stat.periodTypeName)}</td>    
                                    <td>{stat.list[0] ? stat.list[0].attendedCount : 0}</td>
                                    <td>{stat.list[0] ? stat.list[0].firstAttendedString : null}</td>
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

export default UserStatistics;