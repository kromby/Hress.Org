import React, { useState, useEffect } from 'react';
import config from 'react-global-configuration';
import axios from 'axios';
import Post from '../../../../components/post';
import { useAuth } from '../../../../context/auth';
import Movie from '../../components/movie';
import HardheadRating from '../../components/rating';

const MovieOfTheYear = (propsData) => {
    const {authTokens} = useAuth();
    const [nights, setNights] = useState();

    var url = config.get('path') + '/api/hardhead?parentID=' + '5356' + '&attended=8&code=' + config.get('code');

    useEffect(() => {
        const getHardheadUsers = async () => {
            try {
                const response = await axios.get(url);
                setNights(response.data);
            } catch (e) {
                console.error(e);
            }
        }

        getHardheadUsers();
    }, [propsData, url])

    const handelSubmit = async (event) => {

        if(authTokens === undefined) {
            alert("Þú þarf að skrá þig inn");
            return;
        }

        try {
            var url = config.get('path') + "/api/elections/49/voters/2630?code=" +  config.get('code');
            const response = await axios.put(url, {
              LastStepID: propsData.ID
            }, {
                headers: {'Authorization': 'token ' + authTokens.token}
            });
        } catch(e) {
            console.error(e);
        }

        propsData.onSubmit();
    }

    return (  
        <div>
            <Post
                id={propsData.ID}
                title={propsData.Name}
                description={propsData.Description}
                date={propsData.Date}
                dateFormatted={propsData.Year}
                body={
                    <section>
                        <p>
                            Gefðu öllum myndunum sem þú sást einkunn, smellt síðan á Ljúka neðst á síðunni til að halda áfram
                        </p>
                    </section>
                }
            />

            {nights ? nights.map(hardhead => 
                    <Post
                        key={hardhead.ID} 
                        id={hardhead.ID} 
                        title={hardhead.Name}
                        date={hardhead.Date}
                        dateFormatted={hardhead.DateString}
                        body= { <Movie id={hardhead.ID}/> }
                        actions={ <ul className="actions"></ul> }
                        stats={ <HardheadRating id={hardhead.ID} /> }	
                    />
                )
            : null}

            <ul class="actions pagination">
                <li>
                    <a href="#" class="button large next" onClick={handelSubmit}>{"Ljúka " + propsData.Name}</a>
                    {/* <input type="submit" className="button large next" value={"Ljúka " + propsData.Name} disabled={!savingAllowed} /> */}
                </li>
            </ul>
        </div>
    )
}

export default MovieOfTheYear;