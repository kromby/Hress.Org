import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
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

const Election = () => {
    const { authTokens } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();
    const [step, setStep] = useState();



    useEffect(() => {
        if (authTokens === undefined) {
            navigate("/login", { state: { from: location.pathname } });
            return;
        }

        const getNextStep = async () => {
            var url = config.get('apiPath') + '/api/elections/49/voters/access';
            axios.get(url, {
                headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
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

    const getElement = (id, name) => {
        console.log("[Election] id: " + id);
        console.log("[Election] name: " + name);
        if (id === 100) /*Lög og reglur - nýjar og niðurfelldar reglur*/ {
            return <RulesNewOld key={id}
                ID={id}
                Name={name}
                onSubmit={handleSubmit}
            />
        } else if (id === 101) /*Lög og reglur - breytingar*/ {
            return <Rules key={id}
                ID={id}
                Name={name}
                onSubmit={handleSubmit}
            />
        } else if (id === 102) /* Mynd fyrir uppgjörskvöldið */ {
            return <TwentyYearOldMovie key={id}
                ID={id}
                Name={name}
                onSubmit={handleSubmit}
            />
        } else if (id === 360) /*Vonbrigði*/ {
            return <Disappointment key={id}
                ID={id}
                Name={name}
                onSubmit={handleSubmit}
            />
        } else if (id === 361) {
            return <MovieOfTheYear
                key={id}
                ID={id}
                Name={name}
                onSubmit={handleSubmit}
            />
        } else if (id === 362) {
            return <NightOfTheYear key={id}
                ID={id}
                Name={name}
                onSubmit={handleSubmit}
            />
        } else if (id === 363) /*Nýliði*/ {
            return <Post key={id}
                id={id}
                title={name}
            />
        } else if (id === 364) {
            return <HardheadOfTheYear
                key={id}
                ID={id}
                Name={name}
                onSubmit={handleSubmit}
            />
        } else if (id === 5284) {
            return <Stallone
                key={id}
                ID={id}
                Name={name}
                onSubmit={handleSubmit}
            />
        }
    }

    const handleSubmit = async () => {
        window.scrollTo(0, 0);
        window.parent.scrollTo(0, 0);

        try {
            var url = config.get('apiPath') + '/api/elections/49/voters/access';
            const response = await axios.get(url, {
                headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
            });
            setStep(response.data);
        } catch (error) {
            if (error.response.status === 404) {
                console.log("[Election] Access not found");
                setStep(null);
            } else {
                console.error("[Election] Error getting access");
                console.error(error);
            }
        }
    }

    return (
        <div id="main">
            {step ?
                getElement(step.id, step.name)
                : <Post id="0" title="Kosningu lokið" />}
        </div>
    )
}

export default Election;