import axios from "axios";
import { useEffect, useState } from "react"
import { Post } from "../../../components";
import config from 'react-global-configuration';
import UserAwards from "./userAwards";
import { useParams } from "react-router-dom";
import UserStatistics from "./userStatistics";

const HHUsers = () => {
    const params = useParams();

    return (
        <div id="main">
            <UserAwards id={params.id} />
            <UserStatistics id={params.id} />
        </div>
    )
}

export default HHUsers;