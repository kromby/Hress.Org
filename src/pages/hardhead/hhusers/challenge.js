import React, { useEffect, useState } from 'react';
import config from "react-global-configuration";
import axios from 'axios';
import { Post } from '../../../components';
import Author from '../../../components/author';
import UserImage from '../../../components/users/userimage';

const Challenge = ({ id }) => {
    const [challengers, setChallengers] = useState([]);
    const [challenged, setChallenged] = useState([]);

    useEffect(() => {
        const fetchData = async () => {
            //var url = `${config.get('apiPath')}/api/hardhead/statistics/users/${id}/challenges`;
            var url = new URL(`/api/hardhead/statistics/users/${id}/challenges`, config.get('apiPath'));
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

    return (
        <div id="main">
            <Post
                title="Áskoranir"
                body={
                    <div className="row gtr-uniform">
                        <div className="col-3">
                            <h1>Áskorendur</h1>
                                {challengers ? challengers.map(challenger =>
                                    <div key={challenger.user.id}>
                                        <div className="align-center" key={id}>
                                            <UserImage 
                                                id={challenger.user.id} 
                                                username={challenger.user.username} 
                                                profilePhoto={challenger.user.profilePhoto?.href} 
                                            />
                                            Skorað á {challenger.attendedCount} sinnum<br/>
                                            Fyrst {challenger.firstAttendedString}<br/>
                                            Síðast {challenger.lastAttendedString}<br/>
                                        </div>                                        
                                        <br/>
                                    </div>                                    
                                ) : "Smu"}
                        </div>
                        <div className="col-1" />
                        <div className="col-3">
                            <br/><br/><br/><br/><br/>
                            <div className="col-2 align-center" key={id}>
                                <UserImage id={id} username="Zvenni" />
                            </div>
                        </div>
                        <div className="col-2" />
                        <div className="col-3">
                            <h1>Áskoraðir</h1>
                            <ol>
                                {challenged ? challenged.map(challenger =>
                                    <li key={challenger.user.id}>{challenger.user.username} - {challenger.attendedCount} - {challenger.firstAttendedString} - {challenger.lastAttendedString}</li>
                                ) : "Smu"}
                            </ol>
                        </div>
                    </div>
                }
            />
        </div>
    );
};

export default Challenge;