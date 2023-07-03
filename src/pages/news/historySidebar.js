import { useEffect, useState } from "react";
import { ErrorBoundary } from "react-error-boundary";
import { Intro } from '../../components';
import './historySidebar.css';
import YearsSide from './yearsSide';
import queryString from 'query-string';

const HistorySidebar = (propsData) => {
    const [year, setYear] = useState();

    useEffect (() => {
        console.log(propsData);
        const parsed = queryString.parse(propsData.location.search);
        if(parsed.year) {
            setYear(parsed.year);
        }
    }, [propsData])

    function ErrorFallback({ error, resetErrorBoundary }) {
        return (
            <div role="alert">
                <p>Something went wrong:</p>
                <pre>{error.message}</pre>
            </div>
        )
    }

    return (
        <section id="sidebar">
            <Intro logo="https://hress.azurewebsites.net/App_Themes/Default/Images/Logo.png" title="Hress.Org" description="þar sem hressleikinn býr" />

            <ErrorBoundary
                FallbackComponent={ErrorFallback}
                onReset={() => {
                    // reset the state of your app so the error doesn't happen again
                }}
            >
                {/* <!-- Mini Posts --> */}
                {/* <section>
				<div className="mini-posts">
					<!-- Mini Post -->
				</div>
			</section> */}

                {/* <!-- Posts List --> */}
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