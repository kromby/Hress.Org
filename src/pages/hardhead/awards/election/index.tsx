import { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios, { AxiosError } from 'axios';
import Post from '../../../../components/post';
import { useAuth } from '../../../../context/auth';
import Stallone from './stallone';
import HardheadOfTheYear from './hardhead';
import MovieOfTheYear from './movieoftheyear';
import NightOfTheYear from './nightoftheyear';
import Rules from './rules';
import RulesNewOld from './rulesnewold';
import Disappointment from './disappointment';
import TwentyYearOldMovie from './twentyyearoldmovie';
import { useLocation, useNavigate } from 'react-router-dom';
import { Award } from '../../../../types/award';

export interface ElectionModuleProps {
    ID: number;
    Name: string;
    Href?: string;
    onSubmit: () => void;
}

const Election = () => {
    const { authTokens } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();
    const [step, setStep] = useState<Award | undefined>(undefined);

    useEffect(() => {
        if (authTokens === undefined) {
            navigate("/login", { state: { from: location.pathname } });
            return;
        }

        const getNextStep = () => {
            const url = `${config.get('apiPath')}/api/elections/49/voters/access`;
            axios.get(url, {
                headers: { 'X-Custom-Authorization': `token ${authTokens.token}` },
            })
                .then(response => setStep(response.data))
                .catch(error => {
                    if (error.response.status === 404) {
                        console.log("[Election] Access not found");
                    } else {
                        console.error("[Election] Error getting access");
                        console.error(error);
                    }
                })
        }

        document.title = "Harðhausakosningin | Hress.Org";

        getNextStep();
    }, [authTokens])

    const handleSubmit = async () => {
        window.scrollTo(0, 0);
        window.parent.scrollTo(0, 0);

        try {
            const url = `${config.get('apiPath')}/api/elections/49/voters/access`;
            const response = await axios.get(url, {
                headers: { 'X-Custom-Authorization': `token ${authTokens.token}` },
            });
            setStep(response.data);
        } catch (error) {
            if (error instanceof AxiosError && error.response?.status === 404) {
                console.log("[Election] Access not found");
                setStep(undefined);
            } else {
                console.error("[Election] Error getting access");
                console.error(error);
            }
        }
    }

    const getElement = (ID: number, Name: string, Href: string) => {
        if (ID === 100) /*Lög og reglur - nýjar og niðurfelldar reglur*/ {
            return <RulesNewOld key={ID}
                ID={ID}
                Name={Name}
                onSubmit={handleSubmit}
            />
        } else if (ID === 101) /*Lög og reglur - breytingar*/ {
            return <Rules key={ID}
                ID={ID}
                Name={Name}
                onSubmit={handleSubmit}
            />
        } else if (ID === 102) /* Mynd fyrir uppgjörskvöldið */ {
            return <TwentyYearOldMovie key={ID}
                ID={ID}
                Name={Name}
                Href={Href}
                onSubmit={handleSubmit}
            />
        } else if (ID === 360) /*Vonbrigði*/ {
            return <Disappointment key={ID}
                ID={ID}
                Name={Name}
                onSubmit={handleSubmit}
            />
        } else if (ID === 361) {
            return <MovieOfTheYear
                key={ID}
                ID={ID}
                Name={Name}
                Href={Href}
                onSubmit={handleSubmit}
            />
        } else if (ID === 362) {
            return <NightOfTheYear key={ID}
                ID={ID}
                Name={Name}
                Href={Href}
                onSubmit={handleSubmit}
            />
        } else if (ID === 363) /*Nýliði*/ {
            return <Post key={ID}
                id={ID}
                title={Name}
            />
        } else if (ID === 364) {
            return <HardheadOfTheYear
                key={ID}
                ID={ID}
                Name={Name}
                Href={Href}
                onSubmit={handleSubmit} Description={undefined} Date={undefined} Year={undefined}            />
        } else if (ID === 5284) {
            return <Stallone
                key={ID}
                ID={ID}
                Name={Name}
                onSubmit={handleSubmit} Description={undefined} Date={undefined} Year={undefined}            />
        }

        return null;
    }

    return (
        <div id="main">
            {step ?
                getElement(step.id, step.name || "", step.href || "")
                : <Post id="0" title="Kosningu lokið" />}
        </div>
    )
}

export default Election;