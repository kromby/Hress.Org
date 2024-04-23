import { useState } from 'react';
import axios from 'axios';
import config from 'react-global-configuration';

import { useAuth } from '../../context/auth';
import { Navigate, useLocation } from 'react-router-dom';
import { Post } from '../../components';

function Login() {
    const [isError, setIsError] = useState(false);
    const location = useLocation();
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const { authTokens, setAuthTokens } = useAuth();
    const [referer, setReferer] = useState(location.state ? location.state.from : '/');

    
    
    const postLogin = async () => {
                
        var url = config.get('apiPath') + '/api/authenticate';
        try {
            const result = await axios.post(url, { username, password });
            if (result.status === 200) {
                
                

                var balanceUrl = config.get("apiPath") + "/api/users/0/balancesheet";
                const response = await axios.get(balanceUrl, {
                    headers: { 'X-Custom-Authorization': 'token ' + result.data },
                });

                if (response.data.balance && response.data.balance > 0) {
                    setReferer("/profile");
                }

                setAuthTokens({ token: result.data });
            } else {
                setIsError(true);
            }


        } catch (e) {
            console.error(e);
            setIsError(true);
        }
    }

    if (authTokens !== undefined) {
        
        return <Navigate to={referer}/>;
    }

    return (
        <div id="main">
            <section>
                <Post
                    title="Innskráning"
                    body={
                        <div className="row gtr-uniform">
                            <div className="col-6 col-12-xsmall">
                                <input type="text" name="username" id="username" value={username} onChange={e => { setUsername(e.target.value) }} placeholder="Notendanafn" />
                            </div>
                            <div className="col-6 col-12-xsmall">
                                <input type="password" name="password" id="password" value={password} onChange={e => { setPassword(e.target.value) }} placeholder="Lykilorð" />
                            </div>
                            <div className="col-12">
                                <ul className="actions">
                                    <li>
                                        {/* <input type="submit" value="Innskrá" /> */}
                                        <button onClick={() => postLogin()}>Innskrá</button>
                                    </li>
                                </ul>
                            </div>
                            <div className="col-12">
                                {isError && "Ó nei, það vantar meiri hressleika :( Notendanafn eða lykilorð er ekki rétt!"}
                            </div>
                        </div>
                    } />
            </section>
        </div>
    );
}

export default Login;