import axios from "axios";
import config from 'react-global-configuration';
import { useEffect, useState } from "react"

const Author = (propsData) => {
    const[user, setUser] = useState();

    useEffect(() => {
        const getUser = async () => {
            if(propsData.href) {
                var url = config.get('path') + propsData.href + '?code=' + config.get('code');
                try {
                const response = await axios.get(url);
                } catch(e) {
                    console.error(e);
                }                
            } else {
                setUser({
                    ID: propsData.ID,
                    Username: propsData.Username,
                    ProfilePhoto: {
                        Href: propsData.ProfilePhoto
                    }
                });
            }
        }

        getUser();
    }, [propsData])

    return (
        <div>
            {user ?
                <a href={"http://www.hress.org/Gang/Single.aspx?Id=" + user.ID} className="author">
                    <span className="name">{user.Username}</span>
                    {user.ProfilePhoto && user.ProfilePhoto.Href ?
                    <img src={config.get('apiPath') + user.ProfilePhoto.Href} alt={user.Username} /> :
                    null}
                </a>
            : null }
        </div>
    )
}

export default Author;