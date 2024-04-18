import React, { useState } from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import queryString from 'query-string';

import { useAuth } from '../../context/auth';
import { useLocation, useNavigate } from 'react-router-dom';

function Magic() {
    const location = useLocation();
    const navigate = useNavigate();
    const[data, setData] = useState({referer: null, code: null, isLoading: false})    
    const {setAuthTokens} = useAuth();
  
    if(data.isLoading === false) {            
        setData({isLoading: true});
        try {
            const parsed = queryString.parse(location.search);                
            var code = parsed.code;
            
            

            var url = config.get('apiPath') + '/api/authenticate';

            axios.post(url, {code}).then(
                result => {
                    if(result.data.length > 0) {
                        console.log("[Magic] setData " + result.data);                    
                        setAuthTokens({ token: result.data });
                        setData({code: result.data, referer: parsed.path, isLoading: false});
                        navigate(parsed.path)
                    }
                }
            ).catch(function (e) {                
                console.error("[Magic] e1 " + e);
                setAuthTokens();
                setData({isLoading: false});
                navigate("/"); 
            });
        }
        catch(e) {
            console.error("[Magic] e2 " + e);
            setAuthTokens();
            setData({isLoading: false});
            navigate("/");
        }
    }

    return (<div></div>);
}

export default Magic;