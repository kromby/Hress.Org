import axios from "axios";
import { useEffect, useState } from "react"
import { Post } from "../../../components";
import config from 'react-global-configuration';
import UserAwards from "./userAwards";

const HHUsers = (propsData) => { 
    return (
        <div id="main">
            <UserAwards id={propsData.match.params.id} />
        </div>
    )
}

export default HHUsers;