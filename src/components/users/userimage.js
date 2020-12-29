import React, { useState, useEffect } from 'react';
import './userimage.css';

const UserImage = (propsData) => {
    const [userID, setUserID] = useState();
    const [username, setUsername] = useState();
    const [profilePhoto, setProfilePhoto] = useState();

    useEffect(() => {
        setUserID(propsData.id);
        setUsername(propsData.username);
        setProfilePhoto(propsData.profilePhoto);
    }, [propsData])

    return ([
        profilePhoto ?
            <img key="101" className="userimage"
                src={"https://ezhressapi.azurewebsites.net" + profilePhoto + "?code=cCzROHQTlutKpUWOy/BexCj7YkDyFLhGyk4oAEot3eHo1wBgoX/dXQ=="}
                alt={username} />
            : null,
        <p key="102">
            <a href={"http://www.hress.org/Gang/Single.aspx?Id=" + userID}>{username}</a>
        </p>
    ])
}

export default UserImage;