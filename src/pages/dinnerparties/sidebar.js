import { useEffect, useState } from "react"
import config from 'react-global-configuration';
import { Intro } from "../../components"
import axios from "axios";
import DinnerPartyList from "./dinnerPartyList";
import GuestCountSide from "./guestCountSide";
import { ErrorBoundary } from "react-error-boundary";
import TotalGuestsSide from "./totalGuestsSide";

const DinnerPartySidebar = () => {
    const [dinnerParties, setDinnerParties] = useState();

    useEffect(() => {
        const getDinnerParties = async () => {
            var url = config.get("apiPath") + "/api/dinnerparties";
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

    function ErrorFallback({ error }) {
        return (
            <div role="alert">
                <p>Something went wrong:</p>
                <pre>{error.message}</pre>
            </div>
        )
    }

    return (
        <section id="sidebar">
            <Intro logo="/logo.png" title="Hress.Org" description="þar sem hressleikinn býr" />
            <ErrorBoundary
                FallbackComponent={ErrorFallback}
                onReset={() => {}}
            >
                <section>
                    <div className="mini-posts">
                        {/* <!-- Mini Post --> */}
                        <TotalGuestsSide dinnerParties={dinnerParties} />
                        <GuestCountSide dinnerParties={dinnerParties} />
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