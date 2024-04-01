import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import { Link } from 'react-router-dom';
import './userimage.css';

const getImageSrc = (profilePhoto, mode) => {
    const size = mode === "Expanded" ? 60 : mode === "Compact" ? 25 : 40;
    const imagePath = !profilePhoto ? "/api/images/278634/content" : profilePhoto;
    return `${config.get("apiPath")}${imagePath}?height=${size}&width=${size}`;
}

const UserImage = ({ id, username, profilePhoto, mode = "Normal", text }) => {
    return ([
        <Link to={"/hardhead/users/" + id} key="101" className="userimage">
            <img
                src={getImageSrc(profilePhoto, mode)}
                alt={username}
                title={username} />
            <span className={`name${mode}`}>{username}</span>            
        </Link>,
        <div key="102" className="userimagetext">
            {text}
        </div>
    ])
}

export default UserImage;