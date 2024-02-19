import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import { Link } from 'react-router-dom';
import './userimage.css';

const getImageSrc = (profilePhoto) => {
    const imagePath = !profilePhoto ? "/api/images/278634/content" : profilePhoto;
    return `${config.get("apiPath")}${imagePath}?height=50&width=50`;
}

const UserImage = ({ id, username, profilePhoto }) => {
    return ([
        <img key="101" className="userimage"
            src={getImageSrc(profilePhoto)}
            alt={username}  />,
        <p key="102">
            <Link to={"/hardhead/users/" + id}>{username}</Link>
        </p>
    ])
}

export default UserImage;