import React, { useState } from 'react';
import config from 'react-global-configuration';
import axios from "axios";
import * as qs from 'query-string';

import { useAuth } from '../../context/auth';

function Magic(props) {
    const[data, setData] = useState({referer: null, code: null, isLoading: false})    
    //const[setLoggedIn] = useState(false);
    const {setAuthTokens} = useAuth();
  
    if(data.isLoading === false) {            
        setData({isLoading: true});
        try {
            console.log("Magic 2");
            const parsed = qs.parse(props.location.search);                
            var code = parsed.code;
            console.log("Magic code:" + code);
            console.log("Magic path:" + parsed.path);

            var url = config.get('path') + '/api/authenticate?code=' + config.get('code');

            axios.post(url, {code}).then(
                result => {
                    if(result.data.length > 0) {
                        console.log("Magic: setData " + result.data);                    
                        setAuthTokens({ token: result.data });
                        //setLoggedIn(true);
                        setData({code: result.data, referer: parsed.path, isLoading: false});
                        props.history.push(parsed.path)
                    }
                }
            ).catch(function (e) {                
                console.error("Magic " + e);
                setData({isLoading: false});
                props.history.push("/"); 
            });
        }
        catch(e) {
            console.error("Magic " + e);
            setData({isLoading: false});
            props.history.push("/");
        }
    }

    return (<div></div>);
}

export default Magic;