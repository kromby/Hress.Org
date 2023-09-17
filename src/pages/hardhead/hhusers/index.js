import axios from "axios";
import { useEffect, useState } from "react"
import { Post } from "../../../components";
import config from 'react-global-configuration';
import UserAwards from "./userAwards";
import { useParams } from "react-router-dom-v5-compat";

const HHUsers = () => {
    const params = useParams();

    return (
        <div id="main">
            <UserAwards id={params.id} />
        </div>
    )
}

export default HHUsers;