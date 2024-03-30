import UserAwards from "./userAwards";
import { useLocation, useParams } from "react-router-dom";
import UserStatistics from "./userStatistics";
import Challenge from "./challenge";

const HHUsers = () => {
    const params = useParams();
    const location = useLocation();
    const query = new URLSearchParams(location.search);
    const flag = query.get("flag");

    return (
        <div id="main">
            {flag !== "new" ? [
            <UserAwards id={params.id} />,
            <UserStatistics id={params.id} />
            ] : null}
            {flag === "new" || flag === "all" ? <Challenge id={params.id} /> : null}            
        </div>
    )
}

export default HHUsers;