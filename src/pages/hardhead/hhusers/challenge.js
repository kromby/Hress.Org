import { useEffect, useState } from 'react';
import config from "react-global-configuration";
import axios from 'axios';
import { Post } from '../../../components';
import UserImage from '../../../components/users/userimage';

const Challenge = ({ id, username, profilePhoto }) => {
    const [challengers, setChallengers] = useState([]);
    const [challenged, setChallenged] = useState([]);

    useEffect(() => {
        const fetchData = async () => {
            const url = new URL(`/api/hardhead/statistics/users/${id}/challenges`, config.get('apiPath'));
            try {
                const response = await axios.get(url);
                setChallengers(response.data.challengers);
                setChallenged(response.data.challenged);
            } catch (error) {
                console.error('[Challenge.js] Error fetching data:', error);
            }
        };

        fetchData();
    }, [id]);

    if (challengers.length > 0 || challenged > 0) {
        return (
            <div id="main">
                <Post
                    title="Áskoranir"
                    body={
                        <div className="row gtr-uniform">
                            <div className="col-3">
                                <h1>Áskorendur</h1>
                                {challengers ? challengers.slice(0, 5).map(challenger =>
                                    <div className="align-center" key={challenger.user.id}>
                                        <UserImage
                                            id={challenger.user.id}
                                            username={challenger.user.username}
                                            profilePhoto={challenger.user.profilePhoto?.href}
                                            mode="Compact"
                                            text={[
                                                <span key="span01">Skorað á {challenger.attendedCount} sinnum</span>,
                                                <br key="br02" />,
                                                <span key="span03">{challenger.firstAttendedString} - {challenger.lastAttendedString}</span>,
                                            ]}
                                        />
                                        <br />
                                    </div>
                                ) : "Smu"}
                            </div>
                            <div className="col-1" />
                            <div className="col-3">
                                <br /><br /><br /><br /><br /><br /><br />
                                <div className="col-2 align-center" key={id}>
                                    <UserImage id={id} username={username} profilePhoto={profilePhoto} mode="Expanded" />
                                </div>
                            </div>
                            <div className="col-2" />
                            <div className="col-3">
                                <h1>Áskoraðir</h1>
                                {challenged ? challenged.slice(0, 5).map(challenger =>
                                    <div className="align-center" key={challenger.user.id}>
                                        <UserImage
                                            id={challenger.user.id}
                                            username={challenger.user.username}
                                            profilePhoto={challenger.user.profilePhoto?.href}
                                            mode="Compact"
                                            text={[
                                                <span key="span01">Skorað á {challenger.attendedCount} sinnum</span>,
                                                <br key="br02" />,
                                                <span key="span03">{challenger.firstAttendedString} - {challenger.lastAttendedString}</span>,
                                            ]}
                                        />
                                        <br />
                                    </div>
                                ) : "Smu"}
                            </div>
                        </div>
                    }
                />
            </div>
        );
    } else {
        return null;
    }
};

export default Challenge;