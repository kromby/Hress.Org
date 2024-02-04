import axios from "axios";
import config from 'react-global-configuration';
import { useEffect, useState } from "react"

const Author = ({ID, Username, href, ProfilePhoto, UserPath}) => {
    const [user, setUser] = useState();
    const [userPath, setUserPath] = useState("http://www.hress.org/Gang/Single.aspx?Id=");

    useEffect(() => {
        const getUser = async () => {
            if (href) {
                var url = config.get('apiPath') + href;
                try {
                    const response = await axios.get(url);
                } catch (e) {
                    console.error(e);
                }
            } else {
                setUser({
                    id: ID,
                    username: Username,
                    profilePhoto: {
                        href: ProfilePhoto
                    }
                });
            }
        }

        if(UserPath) 
            setUserPath(UserPath);

        if (!user) {
            getUser();
        }
    }, [ID, Username, href, ProfilePhoto, UserPath])

    const getTitle = (username, name) => {
        if (name) {
            return username + " " + name;
        } else 
            return username;
    }

    return (
        <div>
            {user ?
                <a href={userPath + user.id} className="author">
                    <span className="name">{user.username}</span>
                    {user.profilePhoto && user.profilePhoto.href ?
                        <img src={config.get('apiPath') + user.profilePhoto.href + "?height=50&width=50"} alt={user.username} title={getTitle(user.username, user.name)} /> :
                        null}
                </a>
                : null}
        </div>
    )
}

export default Author;