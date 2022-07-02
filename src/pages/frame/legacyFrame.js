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
                src={"https://hress.azurewebsites.net/magic/?code=d6148a6b2f4f456a898de6380a1fa814&path=~" + window.location.pathname}
                width="100%"
                height="2000px"
                id="myLegacyFrame"
             />
        </div>)

        // window.location.replace("https://hress.azurewebsites.net/magic/?code=d6148a6b2f4f456a898de6380a1fa814&path=~" + window.location.pathname);

        // return (
        //     <div id="main" dangerouslySetInnerHTML={{ __html: "<iframe src='https://hress.azurewebsites.net/magic/?code=d6148a6b2f4f456a898de6380a1fa814&path=~" + window.location.pathname + "' width='100%' height='2000px' id='myLegacyFrame'/>"}}>

        //     </div>
        // )
    }
}

export default LegacyFrame;