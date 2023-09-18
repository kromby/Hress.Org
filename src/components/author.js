import axios from "axios";
import config from 'react-global-configuration';
import { useEffect, useState } from "react"

const Author = ({ID, Username, href, ProfilePhoto, UserPath}) => {
    const [user, setUser] = useState();
    const [userPath, setUserPath] = useState("http://www.hress.org/Gang/Single.aspx?Id=");

    useEffect(() => {
        const getUser = async () => {
            if (href) {
                var url = config.get('path') + href + '?code=' + config.get('code');
                try {
                    const response = await axios.get(url);
                } catch (e) {
                    console.error(e);
                }
            } else {
                setUser({
                    ID: ID,
                    Username: Username,
                    ProfilePhoto: {
                        Href: ProfilePhoto
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

    return (
        <div>
            {user ?
                <a href={userPath + user.ID} className="author">
                    <span className="name">{user.Username}</span>
                    {user.ProfilePhoto && user.ProfilePhoto.Href ?
                        <img src={config.get('apiPath') + user.ProfilePhoto.Href} alt={user.Username} /> :
                        null}
                </a>
                : null}
        </div>
    )
}

export default Author;