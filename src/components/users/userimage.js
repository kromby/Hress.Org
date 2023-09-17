import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import { Link } from 'react-router-dom';
import './userimage.css';

const UserImage = ({id, username, profilePhoto}) => {
    return ([
        profilePhoto ?
            <img key="101" className="userimage"
                src={config.get("apiPath") + profilePhoto}
                alt={username} />
            : null,
        <p key="102">
            <Link to={"/hardhead/users/" + id}>{username}</Link>
        </p>
    ])
}

export default UserImage;