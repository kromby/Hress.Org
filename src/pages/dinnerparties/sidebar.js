import { useCallback, useEffect, useState } from "react"
import config from 'react-global-configuration';
import { Intro } from "../../components"
import axios from "axios";
import DinnerPartyList from "./dinnerPartyList";
import GuestCountSide from "./guestCountSide";
import { ErrorBoundary } from "react-error-boundary";
import TotalGuestsSide from "./totalGuestsSide";
import StatsSide from "./statsSide";

const DinnerPartySidebar = () => {
    const [dinnerParties, setDinnerParties] = useState();

    useEffect(() => {
        const getDinnerParties = () => {
            const url = config.get("apiPath") + "/api/dinnerparties";
            axios.get(url).then((response) => {
                setDinnerParties(response.data);
            }).catch((e) => {
                console.error(e);
            })
        }

        if (!dinnerParties) {
            getDinnerParties();
        }
    })

    function errorFallback({ error }) {
        return (
            <div role="alert">
                <p>Something went wrong:</p>
                <pre>{error.message}</pre>
            </div>
        )
    }

    const handleError = useCallback((error) => errorFallback({error}))

    return (
        <section id="sidebar">
            <Intro logo="/logo.png" title="Hress.Org" description="þar sem hressleikinn býr" />
            <ErrorBoundary
                FallbackComponent={handleError}
            >
                <section>
                    <div className="mini-posts">
                        {/* <!-- Mini Post --> */}
                        <TotalGuestsSide dinnerParties={dinnerParties} />
                        <GuestCountSide dinnerParties={dinnerParties} />
                        <StatsSide type="winners" />
                        <StatsSide type="guests" />
                    </div>
                </section>
                <section>
                    <ul className="posts">
                        <DinnerPartyList dinnerParties={dinnerParties} />
                    </ul>
                </section>
            </ErrorBoundary>
        </section>
    )
}

export default DinnerPartySidebar;