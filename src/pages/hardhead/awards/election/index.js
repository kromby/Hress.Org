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
import { Redirect } from 'react-router-dom';
import TwentyYearOldMovie from './twentyyearoldmovie';

const Election = (propsData) => {
    const { authTokens } = useAuth();
    const [step, setStep] = useState();

    if (authTokens === undefined) {
        return <Redirect to={{ pathname: "/login", state: { from: propsData.location.pathname } }} />
    }

    useEffect(() => {
        const getNextStep = async () => {
            try {
                var url = config.get('apiPath') + '/api/elections/49/voters/access';
                const response = await axios.get(url, {
                    headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
                });
                setStep(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        document.title = "Harðhausakosningin | Hress.Org";

        getNextStep();
    }, [propsData, authTokens])

    const getElement = (id, name) => {
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
        // if (authTokens === undefined) {
        //     // TODO Redirect back to main page
        //     return;
        // }

        window.scrollTo(0, 0);
        window.parent.scrollTo(0, 0);

        try {
            var url = config.get('apiPath') + '/api/elections/49/voters/access';
            const response = await axios.get(url, {
                headers: { 'X-Custom-Authorization': 'token ' + authTokens.token },
            });
            setStep(response.data);
        } catch (e) {
            setStep(null);
            console.error(e);
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