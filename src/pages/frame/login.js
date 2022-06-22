import React, { useState } from 'react';
import axios from 'axios';
import config from 'react-global-configuration';

import { useAuth } from '../../context/auth';
import { Redirect } from 'react-router-dom';
import { Post } from '../../components';

function Login(props) {
    const [isError, setIsError] = useState(false);
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const { authTokens, setAuthTokens } = useAuth();
    const referer = props.location.state ? props.location.state.from : '/';

    function postLogin() {        
        console.log("postLogin()");
        console.log("postLogin() - " + username + ":" + password);
        var url = config.get('apiPath') + '/api/authenticate';
        axios.post(url, {username, password}).then(
            result => {
                if(result.status === 200) {
                    console.log("/api/authenticate OK");
                    console.log(result.data);
                    setAuthTokens({ token: result.data });
                    setLoggedIn(true);
                } else {
                    setIsError(true);
                }
            }
        ).catch(e => {
            console.error(e);
            setIsError(true);
        });        
    }

    if(authTokens !== undefined) {
        console.log("referer: " + referer);
        return <Redirect to={referer} />
    }

    return (
        <div id="main">
        <section>
            
            <Post 
                title="Innskráning" 
                body={    
                    <div className="row gtr-uniform">
                        <div className="col-6 col-12-xsmall">
                            <input type="text" name="username" id="username" value={username} onChange={e => {setUsername(e.target.value)}} placeholder="Notendanafn" />
                        </div>
                        <div className="col-6 col-12-xsmall">
                            <input type="password" name="password" id="password" value={password} onChange={e => {setPassword(e.target.value)}} placeholder="Lykilorð" />
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
                            { isError && "Ó nei, það vantar meiri hressleika :( Notendanafn eða lykilorð er ekki rétt!" }
                        </div>
                    </div>
                } />
        </section>
        </div>
    );
}

export default Login;