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

const Election = (propsData) => {
    const { authTokens } = useAuth();
    const [step, setStep] = useState();

    //var url = config.get('path') + '/api/hardhead/awards?code=' + config.get('code');	    

    useEffect(() => {
        if (authTokens === undefined) {
            // TODO Redirect back to main page
            alert("Þú þarft að skrá þig inn");
            return;
        }

        const getNextStep = async () => {
            try {
                var url = config.get('path') + '/api/elections/49/voters/access?code=' + config.get('code');
                const response = await axios.get(url, {
                    headers: { 'Authorization': 'token ' + authTokens.token },
                });
                setStep(response.data);
            } catch (e) {
                console.error(e);
            }
        }

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
        if (authTokens === undefined) {
            // TODO Redirect back to main page
            return;
        }

        window.scrollTo(0, 0);
        window.parent.scrollTo(0, 0);

        try {
            var url = config.get('path') + '/api/elections/49/voters/access?code=' + config.get('code');
            const response = await axios.get(url, {
                headers: { 'Authorization': 'token ' + authTokens.token },
            });
            setStep(response.data);            
        } catch (e) {
            console.error(e);
            alert(e);
        }
    }

    return (
        <div id="main">
            {step ?
                getElement(step.ID, step.Name)
                : <Post id="0" title="Kosningu lokið" />}
        </div>
    )
}

export default Election;