import { useEffect, useState } from "react";
import { ErrorBoundary } from "react-error-boundary";
import { Intro } from '../../components';
import './historySidebar.css';
import YearsSide from './yearsSide';
import queryString from 'query-string';
import { useLocation } from "react-router-dom";

const HistorySidebar = () => {
    const [year, setYear] = useState();
    const location = useLocation();

    useEffect (() => {
        const parsed = queryString.parse(location.search);
        if(parsed.year) {
            setYear(parsed.year);
        }        
    }, [location])

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
                onReset={() => { /* reset the state of your app so the error doesn't happen again */ }}
            >                
                <section>
                    <ul className="posts">
                        <YearsSide year={year} />
                    </ul>
                </section>
            </ErrorBoundary>
        </section>
    )
}

export default HistorySidebar;