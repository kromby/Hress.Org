import { useEffect, useState } from "react";
import { Redirect } from "react-router-dom";
import iframe from "react-iframe";
import { useAuth } from "../../context/auth"

const LegacyFrame = (propsData) => {
    const { authTokens } = useAuth();
    const[isPrivate, setIsPrivate] = useState(false);

    useEffect(() => {
        console.log("Href: '" + window.location.pathname + "'");
    }, [propsData])

    if(isPrivate && authTokens === undefined){
        return <Redirect to='/hardhead' />
    } else {
        return (<div id="main">
            <iframe 
                src={"https://hress.azurewebsites.net" + window.location.pathname}
                width="100%"
                height="2000px"
                id="myLegacyFrame"
                display="initial"
                position="relative"
             />
        </div>)
    }
}

export default LegacyFrame;